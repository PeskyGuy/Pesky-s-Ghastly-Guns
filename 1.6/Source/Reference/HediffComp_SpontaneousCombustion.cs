using Verse;
using RimWorld;

namespace Pesky
{
    public class HediffCompProperties_SpontaneousCombustion : HediffCompProperties
    {
        public float mtbDays = 1f;
        public float fireSize = 0.5f;

        public HediffCompProperties_SpontaneousCombustion()
        {
            this.compClass = typeof(HediffComp_SpontaneousCombustion);
        }
    }

    public class HediffComp_SpontaneousCombustion : HediffComp
    {
        public HediffCompProperties_SpontaneousCombustion Props => (HediffCompProperties_SpontaneousCombustion)props;

        public override void CompPostTick(ref float severityAdjustment)
        {
            if (Pawn.IsHashIntervalTick(60)) // Check roughly every second
            {
                if (Pawn.Spawned && !Pawn.Dead && Rand.MTBEventOccurs(Props.mtbDays, 60000f, 60f))
                {
                    Pawn.TryAttachFire(Props.fireSize, null);
                    if (PawnUtility.ShouldSendNotificationAbout(Pawn))
                    {
                        Messages.Message("PGG_HellboarCombustion".Translate(Pawn.LabelShortCap), Pawn, MessageTypeDefOf.NegativeEvent);
                    }
                }
            }
        }
    }
}
