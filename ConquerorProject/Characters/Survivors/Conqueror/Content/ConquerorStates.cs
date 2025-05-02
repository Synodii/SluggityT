using ConquerorMod.Survivors.Conqueror.SkillStates;

namespace ConquerorMod.Survivors.Conqueror
{
    public static class ConquerorStates
    {
        public static void Init()
        {
            Modules.Content.AddEntityState(typeof(Overpower));

            Modules.Content.AddEntityState(typeof(Eye));

            Modules.Content.AddEntityState(typeof(Advance));

            Modules.Content.AddEntityState(typeof(RopeBackpack));
        }
    }
}
