using System.Collections;
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
            
            if (Input.GetButtonUp("Fire1"))
            {
                IsShooting = false;
            }

            if (Input.GetKeyDown(KeyCode.R))
            {
                TryReload();
            }
        }

        protected override void Shoot()
        {
            Vector3 target;
            
            Vector3 direction = CameraTransform.forward + CurrentDirectionalRecoil;
            if (Physics.Raycast(CameraTransform.position, direction, out RaycastHit hit, definition.ShootingRange, definition.TargetLayerMask))
            {
                Debug.Log($"{definition.GunName} hit {hit.collider.name}");
                target = hit.point;
            }
            else
            {
                target = CameraTransform.position +
                         (CameraTransform.forward + CurrentDirectionalRecoil) * definition.ShootingRange;
            }

            StartCoroutine(ApplyFiringEffects(target, hit));
        }

        private IEnumerator ApplyFiringEffects(Vector3 target, RaycastHit hit)
        {
            GameObject bulletTrail = Instantiate(definition.BulletTrailPrefab, muzzle.position, Quaternion.identity);

            while (bulletTrail && Vector3.Distance(bulletTrail.transform.position, target) > 0.1f)
            {
                bulletTrail.transform.position = Vector3.MoveTowards(bulletTrail.transform.position, target,
                    definition.BulletSpeed * Time.deltaTime);

                yield return null;
            }

            Destroy(bulletTrail);

            if (hit.collider)
            {
                ApplyBulletHitEffect(hit);
            }
        }

        private void ApplyBulletHitEffect(RaycastHit hit)
        {
            Vector3 hitPosition = hit.point + hit.normal * 0.01f;

            GameObject bulletHole = Instantiate(bulletHolePrefab, hitPosition, Quaternion.LookRotation(hit.normal));
            GameObject hitParticle = Instantiate(bulletHitParticlePrefab, hit.point, Quaternion.LookRotation(hit.normal));

            bulletHole.transform.parent = hit.collider.transform;
            hitParticle.transform.parent = hit.collider.transform;

            Destroy(bulletHole, 5f);
            Destroy(hitParticle, 5f);
        }
    }
}
