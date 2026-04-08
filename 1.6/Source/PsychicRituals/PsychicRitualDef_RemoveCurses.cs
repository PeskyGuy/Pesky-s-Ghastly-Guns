using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI.Group;

namespace Pesky
{
    public class PsychicRitualDef_RemoveCurses : PsychicRitualDef_InvocationCircle
    {
        public override List<PsychicRitualToil> CreateToils(PsychicRitual psychicRitual, PsychicRitualGraph graph)
        {
            List<PsychicRitualToil> toils = base.CreateToils(psychicRitual, graph);

            toils.Add(new PsychicRitualToil_RemoveCurses(this.TargetRole));

            return toils;
        }

        public override IEnumerable<string> ConfigErrors()
        {
            foreach (string error in base.ConfigErrors())
            {
                yield return error;
            }
            if (this.TargetRole == null) yield return $"{nameof(targetRole)} is not defined.";
        }
    }
}
