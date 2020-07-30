using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace SR_PluginLoader
{
    enum dCamera_Mode
    {
        FREE=0,
        ORBIT,
    }
    /// <summary>
    /// Provides a free roaming camera for debug purposes.
    /// </summary>
    public class DevCamera : MonoBehaviour
    {
        #region Events
        public event Action onActivate, onDeactivate;
        #endregion

        #region Variables

        /// <summary>
        /// Is the free-fly camera currently active? Is the user currently using it?
        /// </summary>
        public static bool FLYING { get { return cam.enabled; } set { cam.enabled = value; instance.onState_Change(); } }
        private static Camera cam = null, defaultCam=null;
        private static DevCamera instance = null;
        public static DevCamera Instance { get { if (instance == null) { instance = Spawn(); } return instance; } }
        static Vector3 half = new Vector3(0.5f, 0.5f, 0f);
        private dCamera_Mode Mode = dCamera_Mode.FREE;

        /// <summary>
        /// Current x/y rotation values.
        /// (NOTE: These values are added to the identityRot values)
        /// </summary>
        Vector3 rotation;
        Vector2 _smoothMouse;

        /// <summary>
        /// The "Zero Point", as it were, for the camera's rotation values. All rotation from player input is applied relative to these rotation values.
        /// </summary>
        public Vector3 identityRot { get { return _identity_rot; } set { _identity_rot = value; zeroOrientation = Quaternion.Euler(_identity_rot); } }
        private Vector3 _identity_rot = Vector3.zero;
        private Quaternion zeroOrientation = Quaternion.Euler( Vector3.zero );
        /// <summary>
        /// Rotation value limits for the X/Y axi.
        /// </summary>
        public Vector2 rotationLimit = new Vector2(180, 360);
        #endregion

        #region Throw-Away Vars
        dGizmo_Cam defaultCam_Marker = null;
        #endregion

        #region State Variables

        bool EnableFog { get { return enable_fog; } set { enable_fog = value; RenderSettings.fog = value; } }
        bool enable_fog = true;
        #endregion

        #region Editor Variables
        dGizmo_BB View_BB = null;
        Debug_Visualizer Vision = null;
        /// <summary>
        /// The GameObject our cursor is currently over(shown with grey boxes)
        /// </summary>
        GameObject View_Target { get { return view_target; } set { view_target = value; if (View_BB!=null) { View_BB.Dispose(); View_BB = null; } if (value != null) { View_BB = new dGizmo_BB(value, Color.white); View_BB.SetParent(value.transform); } } }
        GameObject view_target = null;
        /// <summary>
        /// The GameObject we are currently locked to and "editing"
        /// </summary>
        GameObject Edit_Target = null;

        bool isEditing { get { return (Edit_Target != null); } }
        #endregion

        #region Settings

        float flySpeed = 0.5f;
        float speedUpRatio = 3.0f;
        float slowDownRatio = 0.5f;
        float ScrollSensitivity = 2.0f;
        float ZoomValue { get { return zoom; } set { zoom = Mathf.Max(0.00001f, value); if (Mode == dCamera_Mode.ORBIT) { transform.localPosition = (Vector3.forward * zoom); } } }
        float zoom = 1f;
        /// <summary>
        /// How many degrees-per-second to rotate when getting rotation input via the keyboard
        /// </summary>
        float Keyboard_Input_Rotation_Speed = 40f;
        GameObject orbit_proxy = null;
        #endregion

        internal static DevCamera Spawn()
        {
            defaultCam = Camera.main;
            var gm = new GameObject("Camera-Flying");
            cam = gm.AddComponent<Camera>();
            cam.tag = "MainCamera";// Tagging a camera as "MainCamera" in unity allows it to become Camera.main when enabled.
            cam.enabled = false;// Disabled by default of course!
            cam.cameraType = CameraType.Game;
            
            var dc = gm.AddComponent<DevCamera>();
            dc.Vision = gm.AddComponent<Debug_Visualizer>();
            gm.AddComponent<Camera_dGizmo_Renderer>();
            var overlay = cam.GetComponent<Overlay>();
            if(overlay!=null) Destroy(overlay);
            //cam.gameObject.AddComponent<SECTR_Member>();
            cam.gameObject.AddComponent<SECTR_RegionLoader>();
            cam.gameObject.AddComponent<SECTR_PointSource>();

            return dc;
        }
        
        public void Toggle() { FLYING = !FLYING; }

        private void onState_Change()
        {
            gameObject.SetActive(cam.enabled);
            if (FLYING)
            {
                cam.CopyFrom(defaultCam);
                SnapToPlayer();

                defaultCam.enabled = false;
                RenderSettings.fog = enable_fog;
                Player.Freeze();
                ViewModel.Toggle(false);
                if (defaultCam_Marker != null) defaultCam_Marker.Dispose();
                defaultCam_Marker = new dGizmo_Cam(defaultCam);
            }
            else
            {
                defaultCam.enabled = true;
                RenderSettings.fog = true;
                Player.Unfreeze();
                ViewModel.Toggle(true);
                if(defaultCam_Marker!=null) defaultCam_Marker.Dispose();
            }

            if(FLYING) onActivate?.Invoke();
            else onDeactivate?.Invoke();
        }

        void LateUpdate()
        {
            if (GameTime.isPaused) return;
            Rotate();
            Move();
            doPicking();
            
            if (Input.GetKeyDown(KeyCode.KeypadEnter))
            {
                Player.Teleport(transform.position, transform.eulerAngles);
                FLYING = false;
            }
            
            if (Input.GetKeyDown(KeyCode.KeypadPeriod))
            {
                if (Mode == dCamera_Mode.ORBIT) ZoomValue = 1f;
                else SnapToPlayer();
            }
            if (Input.GetKeyDown(KeyCode.F)) { EnableFog = !EnableFog; }

            if (isEditing)
            {
                //if (Input.GetKeyDown(KeyCode.Escape)) Start_Editing(null);
                if (Input.GetMouseButtonDown(1)) Start_Editing(null);// right click to stop editing.
            }
            else if (Input.GetMouseButtonDown(0))
            {
                Start_Editing(View_Target);
                View_Target = null;
            }
        }
        
        void Start_Editing(GameObject gm)
        {
            LockTo(gm);
            Vision.Untarget();
            Edit_Target = gm;
            if (gm == null) return;
            Vision.Retarget(Edit_Target);
        }

        void Stop_Editing()
        {
            LockTo(null);
            Vision.Untarget();
            Edit_Target = null;
        }

        /// <summary>
        /// Handle selecting GameObjects on screen as if we were in an editor.
        /// </summary>
        void doPicking()
        {
            if (isEditing)
            {
                if (View_Target != null) View_Target = null;
                return;
            }
            GameObject targ = null;
            Ray ray = Camera.main.ViewportPointToRay(half);
            RaycastHit hit;
            Physics.Raycast(ray, out hit, float.MaxValue);

            Collider collider = hit.collider;
            if (collider != null)
            {
                GameObject gm = hit.collider.gameObject;
                if (gm != null) targ = gm;
            }

            if (targ == Edit_Target) { return; }
            if (targ != View_Target) { View_Target = targ; }
        }
        
        void LockTo(GameObject gm)
        {
            if (gm == null)
            {
                Mode = dCamera_Mode.FREE;
                transform.SetParent(transform, true);// Detach ourself from the orbital proxy but don't remove our current position or rotation, it's jarring and might sp00k someone!
                Set_Rotation(transform);

                if (orbit_proxy != null)
                {
                    orbit_proxy.transform.DetachChildren();
                    UnityEngine.Object.Destroy(orbit_proxy);
                }
                
            }
            else// Orbital camera mode
            {
                Mode = dCamera_Mode.ORBIT;

                var toTarget = (gm.transform.position - transform.position);
                Vector3 lp = gm.transform.InverseTransformPoint(transform.position);
                float dist = lp.magnitude;

                if (orbit_proxy != null) UnityEngine.Object.Destroy(orbit_proxy);
                orbit_proxy = new GameObject("orbit_proxy");
                dGizmo_Axis3D axis = new dGizmo_Axis3D();
                axis.SetParent(orbit_proxy.transform);

                orbit_proxy.transform.SetParent(gm.transform, false);
                orbit_proxy.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
                orbit_proxy.transform.localPosition = Vector3.zero;
                orbit_proxy.transform.up = Vector3.up;// Make sure our proxy isn't tipped over even if our target object is.
                orbit_proxy.transform.LookAt(transform.position);// Rotate to face our current camera position so when we attach the camera our transition will be seamless.

                Set_Rotation(orbit_proxy.transform);

                transform.SetParent(orbit_proxy.transform, false);
                transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
                transform.localPosition = Vector3.zero;
                ZoomValue = dist;
            }
            
        }

        void SnapToPlayer()
        {
            if (isEditing) Stop_Editing();
            SnapTo(defaultCam.transform);
        }

        void SnapTo(Transform trans)
        {
            transform.position = trans.position;
            transform.rotation = trans.rotation;
            Set_Rotation(transform.eulerAngles);
        }
        

        /// <summary>
        /// We want the rotation degrees to represent an offset from our zero point rather than a traditional degree-of-circle value
        /// Meaning instead of storing 270 for the X-Axis we want to translate that to -90 
        /// </summary>
        public static Vector3 FixRot(Vector3 rot)
        {
            rot.x = FixRot(rot.x);
            rot.y = FixRot(rot.y);
            rot.z = FixRot(rot.z);

            return rot;
        }

        public static float FixRot(float rot)
        {
            if (rot > 180f)
            {
                rot = (rot - 180f);// How far over 180 is it
                rot = (rot / 180f);// Normalize
                rot = (1f - rot);// Invert
                rot = (rot * -180f);// Scale
            }

            return rot;
        }

        void Set_Rotation(Vector3 euler)
        {
            reset();
            rotation = FixRot(euler);
        }

        void Set_Rotation(Transform trans)
        {
            Set_Rotation(trans.eulerAngles);
        }

        void reset()
        {
            //if(Mode == dCamera_Mode.ORBIT) identityRot = new Vector3(0, 0, 0);
            //else identityRot = new Vector3(0, 0, 0);
            rotation = Vector2.zero;
            _smoothMouse = Vector2.zero;
        }

        void Move()
        {
            if (!FLYING) return;

            if (Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.RightShift)) { flySpeed *= speedUpRatio; }
            if (Input.GetKeyUp(KeyCode.LeftShift) || Input.GetKeyUp(KeyCode.RightShift)) { flySpeed /= speedUpRatio; }
            if (Input.GetKeyDown(KeyCode.LeftControl) || Input.GetKeyDown(KeyCode.RightControl)) { flySpeed *= slowDownRatio; }
            if (Input.GetKeyUp(KeyCode.LeftControl) || Input.GetKeyUp(KeyCode.RightControl)) { flySpeed /= slowDownRatio; }

            if (Mode == dCamera_Mode.FREE)
            {
                if (Input.GetAxis("Vertical") != 0) { transform.Translate(transform.forward * flySpeed * Input.GetAxis("Vertical"), Space.World); }
                if (Input.GetAxis("Horizontal") != 0) { transform.Translate(transform.right * flySpeed * Input.GetAxis("Horizontal"), Space.World); }
                // Standard Source Engine fly controls.
                if (Input.GetKey(KeyCode.Space)) { transform.Translate(Vector3.up * flySpeed, Space.World); }// Space goes up
                if (Input.GetKey(KeyCode.C)) { transform.Translate(Vector3.up * -flySpeed, Space.World); }// C goes down
            }
            else if(Mode == dCamera_Mode.ORBIT)
            {
                float deltaScroll = Input.GetAxis("Mouse ScrollWheel");
                if (deltaScroll != 0)
                {
                    float scale = (zoom * 0.25f);
                    if (scale < 1f) scale *= scale;
                    scale = Mathf.Clamp(scale, 0.1f, 5f);

                    ZoomValue -= (deltaScroll * ScrollSensitivity * scale);
                }
            }
        }
                
        void Rotate()
        {
            // Get raw mouse input for a cleaner reading on more sensitive mice.
            var mouseDelta = new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y"));

            // Scale input against the sensitivity setting and multiply that against the smoothing value.
            float smoothing = Player.Input.MouseLookSmoothWeight;
            Vector2 sensitivity = Player.Input.MouseLookSensitivity;
            mouseDelta = Vector2.Scale(mouseDelta, new Vector2(sensitivity.x * smoothing, sensitivity.y * smoothing));

            // Interpolate mouse movement over time to apply smoothing delta.
            _smoothMouse.x = Mathf.Lerp(_smoothMouse.x, mouseDelta.x, 1f / smoothing);
            _smoothMouse.y = Mathf.Lerp(_smoothMouse.y, mouseDelta.y, 1f / smoothing);

            // Add the mouse input to our rotation values, but the mouse input X/Y must be swapped because mouse input on it's X(horizontal) axis actually controls the cameras Y axis rotation value.
            rotation.x -= _smoothMouse.y;
            rotation.y += _smoothMouse.x;

            float xLimit = rotationLimit.x;
            float yLimit = rotationLimit.y;
            if (Mode == dCamera_Mode.ORBIT) xLimit = 160f;

            // Clamp rotations for the X-Axis
            if (xLimit < 360) rotation.x = Mathf.Clamp(rotation.x, xLimit * -0.5f, xLimit * 0.5f);
            // Clamp rotation on the Y-Axis
            if (yLimit < 360) rotation.y = Mathf.Clamp(rotation.y, yLimit * -0.5f, yLimit * 0.5f);

            // Now calculate the actual rotation
            Transform tr = transform;
            if (Mode == dCamera_Mode.ORBIT && orbit_proxy != null) { tr = orbit_proxy.transform; }
            
            if (tr == null) return;

            var xRotation = Quaternion.AngleAxis(rotation.x, zeroOrientation * Vector3.right);
            tr.rotation = xRotation;

            tr.rotation *= zeroOrientation;

            var yRotation = Quaternion.AngleAxis(rotation.y, tr.InverseTransformDirection(Vector3.up));
            tr.rotation *= yRotation;

        }
    }
}
