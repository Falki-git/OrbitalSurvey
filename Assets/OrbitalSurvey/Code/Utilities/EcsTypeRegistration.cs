using System;
using System.Reflection;
using OrbitalSurvey.Modules;
using Unity.Entities;

namespace OrbitalSurvey.Utilities
{
    /// <summary>
    /// Registers this mod's part module types with Unity's ECS <see cref="TypeManager"/>.
    ///
    /// Since the Redux DOTS migration, every part module is attached to an ECS entity:
    /// <c>Redux.Ecs.ModuleEntities.Create</c> calls <c>EntityManager.AddComponentObject(entity, module)</c>
    /// for both the view-side <c>PartBehaviourModule</c> and the sim-side
    /// <c>PartComponentModule</c> (which is now an <c>IComponentData</c>). That call resolves the
    /// managed type through the TypeManager, and throws
    /// "Unknown Type: ... All Entities component types must be registered with the TypeManager"
    /// for anything it doesn't know about.
    ///
    /// The game's own module types are covered by the <c>AssemblyTypeRegistry</c> that Unity's ECS
    /// ILPostProcessor bakes into Assembly-CSharp during the player build. A mod assembly compiled
    /// by ThunderKit gets no such registry, so we register our types by hand at load time — the
    /// "component types known only at runtime" path the exception message itself points at.
    ///
    /// <c>TypeManager.TryGetTypeIndex</c> and <c>GetOrCreateTypeIndexUnsafe</c> are internal, hence
    /// the reflection (the assembly publicizer is not used in this project).
    /// </summary>
    internal static class EcsTypeRegistration
    {
        private static readonly ReduxLib.Logging.ILogger Logger =
            ReduxLib.ReduxLib.GetLogger($"OrbitalSurvey|{nameof(EcsTypeRegistration)}");

        private const BindingFlags StaticNonPublic = BindingFlags.Static | BindingFlags.NonPublic;

        // internal static bool TryGetTypeIndex(Type type, out TypeIndex index)
        private static readonly MethodInfo TryGetTypeIndexMethod = typeof(TypeManager).GetMethod(
            "TryGetTypeIndex", StaticNonPublic, null,
            new[] { typeof(Type), typeof(TypeIndex).MakeByRefType() }, null);

        // internal static TypeIndex GetOrCreateTypeIndexUnsafe(Type type)
        //
        // The safe wrapper (GetOrCreateTypeIndex) only creates an index for UnityEngine.Object
        // derived types and rethrows for everything else, so it can't register the managed
        // PartComponentModule. The "Unsafe" variant builds the TypeInfo for any type, and still
        // fills in the descendant map for UnityEngine.Object types.
        private static readonly MethodInfo GetOrCreateTypeIndexUnsafeMethod = typeof(TypeManager).GetMethod(
            "GetOrCreateTypeIndexUnsafe", StaticNonPublic, null, new[] { typeof(Type) }, null);

        /// <summary>
        /// Registers the mod's module types. Idempotent, and safe to call before the ECS world
        /// exists. Must run before any part carrying Module_OrbitalSurvey is loaded (OAB or Flight).
        /// </summary>
        internal static void RegisterModuleTypes()
        {
            if (TryGetTypeIndexMethod == null || GetOrCreateTypeIndexUnsafeMethod == null)
            {
                Logger.LogError(
                    "Could not find TypeManager.TryGetTypeIndex / TypeManager.GetOrCreateTypeIndexUnsafe. " +
                    "Unity.Entities has changed; parts using Module_OrbitalSurvey will fail to load.");
                return;
            }

            // No-op when the TypeManager is already up, which it normally is by this point.
            TypeManager.Initialize();

            Register(typeof(Module_OrbitalSurvey));
            Register(typeof(PartComponentModule_OrbitalSurvey));
        }

        private static void Register(Type type)
        {
            try
            {
                var args = new object[] { type, null };
                if ((bool)TryGetTypeIndexMethod.Invoke(null, args))
                {
                    Logger.LogDebug($"'{type.Name}' is already known to the ECS TypeManager.");
                    return;
                }

                // TypeIndex.ToString() reads DebugTypeName, which is empty in a release player
                // build, so log the raw index instead of the (always "null") string form.
                var typeIndex = (TypeIndex)GetOrCreateTypeIndexUnsafeMethod.Invoke(null, new object[] { type });
                Logger.LogInfo($"Registered '{type.Name}' with the ECS TypeManager (index {typeIndex.Index}).");
            }
            catch (Exception ex)
            {
                Logger.LogError($"Failed to register '{type.FullName}' with the ECS TypeManager.\n{ex}");
            }
        }
    }
}
