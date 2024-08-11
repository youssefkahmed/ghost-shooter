using UnityEngine;

namespace Ghost.Weapons
{
    public class Pistol : Gun
    {
        protected override void Update()
        {
            base.Update();

            if (Input.GetButtonDown("Fire1"))
            {
                TryShoot();
            }

            if (Input.GetKeyDown(KeyCode.R))
            {
                TryReload();
            }
        }

        protected override void Shoot()
        {
            var direction = CameraTransform.forward + CurrentDirectionalRecoil;
            if (Physics.Raycast(CameraTransform.position, direction, out var hit, definition.ShootingRange, definition.TargetLayerMask))
            {
                Debug.Log($"{definition.GunName} hit {hit.collider.name}");
            }
        }
    }
}
