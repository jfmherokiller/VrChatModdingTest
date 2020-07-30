using System.Collections.Generic;

namespace SR_PluginLoader
{
    public static class HOOK_SHAS
    {
        /// <summary>
        /// Maps hook names to a hash of the targeted function when we last verified it gets hooked correctly.
        /// </summary>
        public static Dictionary<string, string> SAFE_HASHES = new Dictionary<string, string>()
        {
            { "CellDirector.Update", "1ad38ab4a967a8e18f8720110cb498a38b418592" },
            { "CoopUI.CreatePurchaseUI", "8d2a950c8947c01dac30d65fb4f58a74f536f521" },
            { "CoopUI.Demolish", "ed442d28d60e8c10b5d4d3c375b9405661721592" },
            { "CorralUI.CreatePurchaseUI", "5b1649f1d21b6893daba2285476a445c345a3402" },
            { "CorralUI.Demolish", "024c7fe74720c3582b5aa6d64172cdb878714b17" },
            { "DirectedActorSpawner.Spawn", "aed3b6a2f690e98aa5be9d965f46f621c16f4637" },
            { "DirectedActorSpawner.Start", "4fe291c922bd953740d5840054291bf1de4b5410" },
            { "EconomyDirector.InitForLevel", "ca2c176a436e8707cc4fa6c8559cf12ed9444069" },
            { "EconomyDirector.Update", "232135706f624f9cf56579c9a2bafb816e8a2a67" },
            { "EmptyPlotUI.CreatePurchaseUI", "8774e85bc9be4d6ac8e88d16faf914024ef0a241" },
            { "GameData.AvailableGames", "143839541acf753f7a00362a103905cce663aeaa" },
            { "GameData.Load", "7a4dcc5ac085acf3c1f7e486ae42edaab69ed5ad" },
            { "GameData.Save", "0bab3eacc68042c9056873fbb4248f44efbbe640" },
            { "GameData.ToPath", "d5e79cdbc333dba8d120b73c92cd7d29f5b1eb92" },
            { "GardenCatcher.Awake", "91b9a6ee9d210a133f6de06a909e1e4cc5346614" },
            { "GardenCatcher.OnTriggerEnter", "96acf7cdea22b7be60a189573ae1c92449c9fa03" },
            { "GardenUI.CreatePurchaseUI", "1beb19f01770c133ac94ded75c74cdc14be94ae2" },
            { "GardenUI.Demolish", "963abcd8d5ecccd4c785fd13b9cd0a91e6ace34b" },
            { "Identifiable.Awake", "f7e7ea634cac44e16554e6f90662176aa63e205c" },
            { "IncineratorUI.CreatePurchaseUI", "ad691d85f37b75eab172022b01ef11b71f907e52" },
            { "IncineratorUI.Demolish", "41a37193a6bcff21d2976a7d6b09f0007af78877" },
            { "LandPlot.SetUpgrades", "231f2fdcf44d31a82c73595ecfa145618cc24acc" },
            { "LockOnDeath.LockUntil", "295eb4f721f03a222946105bf6eb0932b1880d9c" },
            { "LockOnDeath.Update", "2b9c4b9c1baf12f8e6961b102995a557865dc5be" },
            { "PersonalUpgradeUI.CreatePurchaseUI", "c6120825494d2f5092cc6d2b5fca7e69791c4fce" },
            { "PlayerDeathHandler.OnDeath", "750b7c78c15db3861a02e2f45b0ca497022ce5f2" },
            { "PlayerState.AddCurrency", "7c0454e3ce6959f852a22d046734fb8c6d867f31" },
            { "PlayerState.AddRads", "e58780edc19bc4763ca0bc83f144942578cc5a1b" },
            { "PlayerState.ApplyUpgrade", "1056d6ca685277cfe3c1d16e20a5f84f5e7438eb" },
            { "PlayerState.CanGetUpgrade", "bab916084658dc735f1362ab3dd2a9bc720928b7" },
            { "PlayerState.Damage", "e1548891d4c58bcd0c021335fd0d7a1cf421c2f9" },
            { "PlayerState.SetEnergy", "8b67962e618ce4a8b4713227abac534e093d2075" },
            { "PlayerState.SpendEnergy", "f8a9f2fb6fa00359cbafe31ae45bcc978fec8ebe" },
            { "PondUI.CreatePurchaseUI", "22964f37651cfae2b9ab0f87bb6f13328cfacb79" },
            { "PondUI.Demolish", "a0304670b66daec4d8e3775d5f0c2df78ba96f4e" },
            { "SiloCatcher.OnTriggerEnter", "81f23c56e9086ab93dcbd8ff073633ed4c8e4b47" },
            { "SiloCatcher.OnTriggerStay", "9de139959cc383ef3cb6fae42adccb0b63701ab7" },
            { "SiloUI.CreatePurchaseUI", "c33c4546c429cc8b03a4c8264fa628d2e266ec1e" },
            { "SiloUI.Demolish", "29e9116f8840cb855a6579bc0e84cdb16c0eabbb" },
            { "SpawnResource.Start", "8743b7bb300ce26f221667ef6adaabdd65215e78" },
            { "Vacuumable.canCapture", "bfb4cf5671ac0d1a73b248e7568620c572b1baa6" },
            { "Vacuumable.capture", "67614dbe5e993ef0de402e7d5190f9656f6b1d73" },
            { "WeaponVacuum.ConsumeVacItem", "2aaa9a526a2323f883b311ae898e774a320fde12" },
            { "WeaponVacuum.Update", "9755db90852ce947e15e614cd352a02d81d32e8a" },
        };
    }
}
