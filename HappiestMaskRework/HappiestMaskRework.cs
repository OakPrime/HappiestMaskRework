using BepInEx;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using System;
using RoR2;
using R2API;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace HappiestMaskRework
{

    //This is an example plugin that can be put in BepInEx/plugins/ExamplePlugin/ExamplePlugin.dll to test out.
    //It's a small plugin that adds a relatively simple item to the game, and gives you that item whenever you press F2.

    //This attribute is required, and lists metadata for your plugin.
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]

    //This is the main declaration of our plugin class. BepInEx searches for all classes inheriting from BaseUnityPlugin to initialize on startup.
    //BaseUnityPlugin itself inherits from MonoBehaviour, so you can use this as a reference for what you can declare and use in your plugin class: https://docs.unity3d.com/ScriptReference/MonoBehaviour.html
    public class HappiestMaskRework : BaseUnityPlugin
    {
        //The Plugin GUID should be a unique ID for this plugin, which is human readable (as it is used in places like the config).
        //If we see this PluginGUID as it is on thunderstore, we will deprecate this mod. Change the PluginAuthor and the PluginName !
        public const string PluginGUID = PluginAuthor + "." + PluginName;
        public const string PluginAuthor = "OakPrime";
        public const string PluginName = "HappiestMaskRework";
        public const string PluginVersion = "1.1.0";

        private readonly Dictionary<string, string> DefaultLanguage = new Dictionary<string, string>();

        //The Awake() method is run at the very start when the game is initialized.
        public void Awake()
        {
            Log.Init(Logger);
            try
            {
                RoR2Application.onLoad += () =>
                {
                    var list = RoR2Content.Items.GhostOnKill.tags.ToList();
                    list.Add(ItemTag.CannotCopy);
                    RoR2Content.Items.GhostOnKill.tags = list.ToArray();
                };
                IL.RoR2.CharacterBody.OnInventoryChanged += (il) =>
                {
                    ILCursor c = new ILCursor(il);
                    c.TryGotoNext(
                        x => x.MatchLdarg(out _),
                        x => x.MatchLdarg(out _),
                        x => x.MatchCallOrCallvirt<RoR2.CharacterBody>(nameof(RoR2.CharacterBody.inventory))
                    );
                    c.Emit(OpCodes.Ldarg_0);
                    c.EmitDelegate<Action<RoR2.CharacterBody>>(body =>
                    {
                        body.AddItemBehavior<HappiestMaskBehavior>(body.inventory.GetItemCount(RoR2Content.Items.GhostOnKill));
                    });
                };
                IL.RoR2.GlobalEventManager.OnCharacterDeath += (il) =>
                {
                    ILCursor c = new ILCursor(il);
                    c.TryGotoNext(
                        x => x.MatchLdcR4(7.0f)
                    );
                    c.RemoveRange(3);
                    c.Emit(OpCodes.Ldarg_1);
                    c.EmitDelegate<Func<DamageReport, bool>>((damageReport) =>
                    {
                        HappiestMaskBehavior maskBehavior = damageReport.attacker.GetComponent<HappiestMaskBehavior>();
                        return maskBehavior != null && !maskBehavior.HasGhost();
                    });
                    c.Index += 3;
                    c.Remove();
                    c.Index++;
                    c.Remove();
                    c.Index++;
                    c.Remove();
                    c.Emit(OpCodes.Ldarg_1);
                    c.EmitDelegate<Action<CharacterBody, DamageReport>>((ghostBody, damageReport) =>
                    {
                        if (ghostBody != null)
                        {
                            damageReport.attacker.GetComponent<HappiestMaskBehavior>().SetGhost(ghostBody);
                            ghostBody.master.inventory.GiveItem(RoR2Content.Items.BoostDamage, 150 * (damageReport.attackerBody.inventory.GetItemCount(RoR2Content.Items.GhostOnKill) - 1));
                        }   
                    });
                };
                this.UpdateText();
            }
            catch (Exception e)
            {
                Logger.LogError(e.Message + " - " + e.StackTrace);
            }
        }
        private void UpdateText()
        {
            this.ReplaceString("ITEM_GHOSTONKILL_DESC", "Killing an enemy will spawn a ghost of the killed enemy with <style=cIsDamage>1500%</style>"
                + "<style=cStack>(+1500% per stack)</style> damage for <style=cIsDamage>30s</style>. ");
            this.ReplaceString("ITEM_GHOSTONKILL_PICKUP", "Killing an enemy spawns a friendly ghost of them.");
        }


        private void ReplaceString(string token, string newText)
        {
            this.DefaultLanguage[token] = Language.GetString(token);
            LanguageAPI.Add(token, newText);
        }
    }
}
