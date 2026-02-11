using UnityEngine;
using Verse;
using RimWorld;
using Verse.Sound;

namespace NoNeedAbandonedSettlement
{
    public class NNMod : Mod
    {
        public static NNSettings Settings;

        public NNMod(ModContentPack content) : base(content)
        {
            Settings = GetSettings<NNSettings>();
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            Listing_Standard listing = new Listing_Standard();
            listing.Begin(inRect);

            // Local, not serialized — remembers the last used value between toggles
            int lastUsedCooldown = Settings.cooldownDays == 0 ? 15 : Settings.cooldownDays;
            bool enableCooldown = Settings.cooldownDays > 0;

            listing.CheckboxLabeled(
                "DAS_AutoRemoveLabel".Translate(),
                ref Settings.autoRemoveImmediately,
                "DAS_AutoRemoveTooltip".Translate()
            );

            if (Settings.autoRemoveImmediately)
            {
                listing.CheckboxLabeled(
                    "DAS_AutoRemoveRespectCooldownLabel".Translate(),
                    ref Settings.autoRemoveRespectCooldown,
                    "DAS_AutoRemoveRespectCooldownTooltip".Translate()
                );
            }

            listing.Gap();

            listing.CheckboxLabeled(
                "DAS_EnableCooldownLabel".Translate(),
                ref enableCooldown,
                "DAS_EnableCooldownTooltip".Translate()
            );

            if (enableCooldown)
            {
                if (Settings.cooldownDays == 0)
                    Settings.cooldownDays = lastUsedCooldown;

                listing.Label("DAS_CooldownSliderLabel".Translate(Settings.cooldownDays), -1f,
                    "DAS_CooldownSliderTooltip".Translate());

                //Settings.cooldownDays = (int)listing.Slider(Settings.cooldownDays, 1, 30);
                Settings.cooldownDays = Mathf.RoundToInt(listing.Slider(Settings.cooldownDays, 1, 180)); //Smother Slider?


                if (listing.ButtonText("Reset to default (15 days)"))
                {
                    Settings.cooldownDays = 15;
                    SoundDefOf.Tick_High.PlayOneShotOnCamera();
                }

                listing.Gap();
                listing.Label("DAS_CooldownInfo".Translate());
            }
            else
            {
                listing.Label("DAS_CooldownDisabledLabel".Translate(), tooltip: "DAS_EnableCooldownTooltip".Translate());
                Settings.cooldownDays = 0;
            }

            listing.End();
        }


        public override string SettingsCategory() => "Directly Abandon Settlement";
    }

    public class NNSettings : ModSettings
    {
        public int cooldownDays = 15;
        public bool autoRemoveImmediately = false;
        public bool autoRemoveRespectCooldown = true;


        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref cooldownDays, "cooldownDays", 15);
            Scribe_Values.Look(ref autoRemoveImmediately, "autoRemoveImmediately", false);
            Scribe_Values.Look(ref autoRemoveRespectCooldown, "autoRemoveRespectCooldown", true);
        }
    }
}
