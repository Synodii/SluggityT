using ConquerorMod.Survivors.Conqueror.SkillStates;

namespace ConquerorMod.Survivors.Conqueror
{
    public static class ConquerorStates
    {
        public static void Init()
        {
            Modules.Content.AddEntityState(typeof(OverpowerCharge));

            Modules.Content.AddEntityState(typeof(Eye));

            Modules.Content.AddEntityState(typeof(AxeCombo1));

            Modules.Content.AddEntityState(typeof(RopeBackpackCharge));
        }
    }
}
