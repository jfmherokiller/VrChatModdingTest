using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace SR_PluginLoader
{

    public class ModelData_Header
    {
        public List<string[]> PHYS_LIST = new List<string[]>();
        public List<string[]> STATE_LIST = new List<string[]>();
        public List<string[]> MESH_LIST = new List<string[]>();
        public List<string[]> ATTACHMENT_POINT_LIST = new List<string[]>();

        public ModelData_Header() { }

        internal void Push_State(string fullName, string keyName) { STATE_LIST.Add(new string[] { fullName, keyName.ToLowerInvariant() }); }
        internal void Push_Phys(string fullName, string keyName) { PHYS_LIST.Add(new string[] { fullName, keyName.ToLowerInvariant() }); }
        internal void Push_Mesh(string fullName, string keyName) { MESH_LIST.Add(new string[] { fullName, keyName.ToLowerInvariant() }); }
        internal void Push_Attachment_Spot(string fullName, string keyName) { ATTACHMENT_POINT_LIST.Add(new string[] { fullName, keyName.ToLowerInvariant() }); }
    }
    /// <summary>
    /// Provides details and helper functions for common tasks associated with Models loaded through the PluginLoader
    /// </summary>
    public class ModelData : MonoBehaviour
    {
        /// <summary>
        /// Should we draw all of the attachment points (For Science)?
        /// </summary>
        private bool FLAG_DRAW_ATTACHMENT_POINTS = false;

        private static Material lineMaterial = null;

        /// <summary>
        /// A map of all Attachment pos groups within the model.
        /// Attachment Points are specified by prefixing a group name with "POS_", these groups will not render normally nor will they have a mesh. They are just empty GameObject's which serve to hold a relative position within the parent model.
        /// </summary>
        private Dictionary<string, GameObject> ATTACHMENT_POINTS = null;
        private Dictionary<string, GameObject> PHYS = null;
        private Dictionary<string, GameObject> STATES = null;
        private Dictionary<string, GameObject> MESHES = null;


        #region SETUP
        private void Awake()
        {
            // Use this game object's name to find it's Data_Header in the static cache.
            string prefabName = base.name.Replace("(Clone)", "").TrimEnd();
            ModelData_Header header = ModelHelper.data_cache[prefabName];
            // Assemble this objects dictionarys
            Setup_Dict(header.MESH_LIST, out MESHES);
            Setup_Dict(header.STATE_LIST, out STATES);
            Setup_Dict(header.PHYS_LIST, out PHYS);
            Setup_Dict(header.ATTACHMENT_POINT_LIST, out ATTACHMENT_POINTS);
        }

        private void Start()
        {
            TryCreateLineMat();
        }

        private void Setup_Dict(List<string[]> names_list, out Dictionary<string, GameObject> dict)
        {
            dict = new Dictionary<string, GameObject>();
            foreach (string[] strs in names_list)
            {
                string fullName = strs[0];
                string name = strs[1];

                var child = base.gameObject.transform.Find(fullName);
                if(child == null)
                {
                    SLog.Info("Unable to find child named: \"{0}\"", fullName);
                    continue;
                }
                GameObject gObj = child.gameObject;

                if (!dict.ContainsKey(name)) dict.Add(name, gObj);
                else SLog.Info("Duplicate entry for child named: \"{0}\"", name);
            }
            //PLog.Info("Dictionary<> {0} entrys. {1}", dict.Keys.Count, String.Join(", ", dict.Keys.ToArray()));
        }

        private void TryCreateLineMat()
        {
            if (lineMaterial == null)
            {
                // Unity has a built-in shader that is useful for drawing
                // simple colored things.
                var shader = Shader.Find("Hidden/Internal-Colored");
                lineMaterial = new Material(shader);
                lineMaterial.hideFlags = HideFlags.HideAndDontSave;
                // Turn on alpha blending
                lineMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                lineMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                // Turn backface culling off
                lineMaterial.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
                // Turn off depth writes
                lineMaterial.SetInt("_ZWrite", 0);
            }
        }
        #endregion

        #region Internals
        #endregion

        #region Public Accessors
        /// <summary>
        /// Hides all STATE_ models and shows the one specified.
        /// </summary>
        /// <param name="sIdx">The STATE number to show.</param>
        /// <returns>GameObject: the STATE model we have shown.</returns>
        public GameObject Switch_State(int sIdx)
        {
            if (STATES == null) throw new Exception("STATES Dictionary not ready!");
            foreach (var kvp in STATES)
            {
                string stName = kvp.Key;
                Transform objTrans = kvp.Value.transform;
                if (objTrans != null) objTrans.gameObject.SetActive(false);
            }

            string state = String.Format("STATE_{0}", sIdx).ToLowerInvariant();
            if (!this.STATES.ContainsKey(state)) return null;

            GameObject sObj = STATES[state];
            if (sObj == null) return null;

            sObj.SetActive(true);
            return sObj;
        }
        
        /// <summary>
        /// Returns the GameObject instance for a specified STATE group.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public GameObject Get_State(int sIdx)
        {
            if (STATES == null) throw new Exception("STATES Dictionary not ready!");
            string state = String.Format("STATE_{0}", sIdx).ToLowerInvariant();
            if (!this.STATES.ContainsKey(state)) return null;

            return STATES[state];
        }

        /// <summary>
        /// Returns the GameObject instance for a specified PHYS group.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public GameObject Get_Phys(string name)
        {
            string keyStr = name.ToLowerInvariant();
            if (PHYS == null) throw new Exception("PHYS Dictionary not ready!");
            if (!PHYS.ContainsKey(keyStr)) return null;

            return PHYS[keyStr];
        }

        /// <summary>
        /// Returns the GameObject instance for a specified MESH group.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public GameObject Get_Mesh(string name)
        {
            string keyStr = name.ToLowerInvariant();
            if (MESHES == null) throw new Exception("MESHES Dictionary not ready!");
            if (!MESHES.ContainsKey(keyStr))
            {
                SLog.Info("No MESH group named \"{0}\" could be found!", keyStr);
                return null;
            }

            return MESHES[keyStr];
        }

        /// <summary>
        /// Returns the GameObject instance for a specified ATTACHMENT_POINT group.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public GameObject Get_Attachment_Point(string name)
        {
            string keyStr = name.ToLowerInvariant();
            if (ATTACHMENT_POINTS == null) throw new Exception("ATTACHMENT_POINTS Dictionary not ready!");
            if (!ATTACHMENT_POINTS.ContainsKey(keyStr))
            {
                SLog.Info("No ATTACHMENT_POINTS group named \"{0}\" could be found!", keyStr);
                return null;
            }

            return ATTACHMENT_POINTS[keyStr];
        }
        #endregion

        #region DEBUG
        /// <summary>
        /// If <c>True</c> then all of the models attachment points will be rendered visually overtop it within the game world
        /// Note: Currently Broken, rendered items are not at the correct locations (NOT EVEN CLOSE!)
        /// </summary>
        /// <param name="b"></param>
        public void Show_Attachment_Points(bool b) { FLAG_DRAW_ATTACHMENT_POINTS = b; }
        #endregion

        #region Private Parts
        private void OnRenderObject()
        {
            if (!base.isActiveAndEnabled) return;
            if (!base.gameObject.activeSelf) return;
            if (transform == null) return;

            GL.PushMatrix();
            //GL.LoadIdentity();
            GL.MultMatrix(transform.localToWorldMatrix);

            lineMaterial.SetPass(0);
            GL.Color(Color.green);
            GL.Begin(GL.TRIANGLES);
            if(FLAG_DRAW_ATTACHMENT_POINTS)
            {
                foreach (KeyValuePair<string, GameObject> kvp in ATTACHMENT_POINTS)
                {
                    GameObject point = kvp.Value;
                    if (point == null)
                    {
                        SLog.Info("attachment point is null!");
                        continue;
                    }
                    if(point.transform == null)
                    {
                        SLog.Info("attachment point transform is null!");
                        continue;
                    }
                    // Draw a triangle centered on this point
                    GL_Render_Triangle(point.transform.position);
                }
            }
            GL.End();

            GL.PopMatrix();
        }

        private void GL_Render_Triangle(Vector3 pos, float size = 0.8f)
        {
            Vector3 A = new Vector3(pos.x, pos.y + size, pos.z);
            Vector3 B = new Vector3(pos.x + size, pos.y - size, pos.z - size);
            Vector3 C = new Vector3(pos.x - size, pos.y - size, pos.z + size);

            GL.Vertex(A);
            GL.Vertex(B);
            GL.Vertex(C);
        }

        private void GL_Render_Triangle_Lines(Vector3 pos, float size = 0.8f)
        {
            Vector3 A = new Vector3(pos.x, pos.y + size, pos.z);
            Vector3 B = new Vector3(pos.x + size, pos.y - size, pos.z - size);
            Vector3 C = new Vector3(pos.x - size, pos.y - size, pos.z + size);

            GL.Vertex(A);
            GL.Vertex(B);

            GL.Vertex(B);
            GL.Vertex(C);

            GL.Vertex(C);
            GL.Vertex(A);
        }
        #endregion
    }
}
