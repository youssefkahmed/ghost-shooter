using UnityEngine;

namespace Ghost.Weapons
{
    [CreateAssetMenu(fileName = "New Gun Definition", menuName = "Ghost/Definitions/Guns/Gun Definition")]
    public class GunDefinition : ScriptableObject
    {
        [Header("General")]
        [field: SerializeField] public string GunName { get; private set; }
        [field: SerializeField] public LayerMask TargetLayerMask { get; private set; }
        
        [Header("Firing")]
        [field: SerializeField] public float ShootingRange { get; private set; }
        [field: SerializeField] public float FireRate { get; private set; }
        
        [Header("Reloading")]
        [field: SerializeField] public float MagazineSize { get; private set; }
        [field: SerializeField] public float ReloadTime { get; private set; }
        
        [Header("Aim Recoil")]
        [field: SerializeField] public float AimRecoilAmount { get; private set; }
        [field: SerializeField] public Vector2 MaxAimRecoil { get; private set; }
        [field: SerializeField] public float AimRecoilSpeed { get; private set; }
        [field: SerializeField] public float ResetAimRecoilSpeed { get; private set; }
        
        [Header("Directional Recoil")]
        [field: SerializeField] public float DirectionalRecoilAmount { get; private set; }
        [field: SerializeField] public Vector2 MaxDirectionalRecoil { get; private set; }
        [field: SerializeField] public float DirectionalRecoilSpeed { get; private set; }
        [field: SerializeField] public float ResetDirectionalRecoilSpeed { get; private set; }
        
        [Header("VFX")]
        [field: SerializeField] public GameObject BulletTrailPrefab { get; private set; }
        [field: SerializeField] public float BulletSpeed { get; private set; }
    }
}
