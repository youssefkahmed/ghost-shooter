using System.Collections;
using Ghost.Player;
using UnityEngine;

namespace Ghost.Weapons
{
    public abstract class Gun : MonoBehaviour
    {
        [SerializeField] protected GunDefinition definition;
        [SerializeField] protected Transform muzzle;
        [SerializeField] protected GameObject muzzleFlash;
        [SerializeField] protected GameObject bulletHolePrefab;
        [SerializeField] protected GameObject bulletHitParticlePrefab;

        protected Transform CameraTransform;
        protected bool IsShooting;
        
        private PlayerController _playerController;
        
        private float _currentAmmo;
        private float _nextTimeToFire;

        protected Vector3 CurrentDirectionalRecoil = Vector3.zero;
        private Vector3 _targetDirectionalRecoil = Vector3.zero;
        
        private bool _isReloading;

        private void Start()
        {
            _currentAmmo = definition.MagazineSize;

            _playerController = transform.root.GetComponent<PlayerController>();
            CameraTransform = _playerController.GetCamera();
        }

        protected virtual void Update()
        {
            _playerController.ResetAimRecoil(definition);
            ResetDirectionalRecoil();

            ToggleMuzzleFlash(IsShooting);
        }

        #region Reloading

        protected bool TryReload()
        {
            if (_isReloading || _currentAmmo >= definition.MagazineSize)
            {
                return false;
            }
            
            StartCoroutine(Reload());
            return true;
        }

        private IEnumerator Reload()
        {
            _isReloading = true;

            yield return new WaitForSeconds(definition.ReloadTime);

            _currentAmmo = definition.MagazineSize;
            _isReloading = false;
        }

        #endregion

        #region Shooting

        protected bool TryShoot()
        {
            if (_isReloading)
            {
                return false;
            }

            if (_currentAmmo <= 0f)
            {
                return false;
            }

            if (Time.time < _nextTimeToFire)
            {
                return false;
            }
            
            _nextTimeToFire = Time.time + 1 / definition.FireRate;
            HandleShoot();
            return true;
        }

        private void HandleShoot()
        {
            IsShooting = true;
            
            _currentAmmo--;
            Shoot();
            
            Debug.Log("Shot!");
            _playerController.ApplyAimRecoil(definition);
            ApplyDirectionalRecoil();
        }

        protected abstract void Shoot();

        private void ToggleMuzzleFlash(bool activate)
        {
            muzzleFlash?.SetActive(activate);
        }

        #endregion

        #region Recoil

        private void ApplyDirectionalRecoil()
        {
            var recoilX = Random.Range(-definition.MaxDirectionalRecoil.x, definition.MaxDirectionalRecoil.x) * definition.DirectionalRecoilAmount;
            var recoilY = Random.Range(-definition.MaxDirectionalRecoil.y, definition.MaxDirectionalRecoil.y) * definition.DirectionalRecoilAmount;

            _targetDirectionalRecoil += new Vector3(recoilX, recoilY, 0);
            CurrentDirectionalRecoil = Vector3.MoveTowards(CurrentDirectionalRecoil, _targetDirectionalRecoil, definition.DirectionalRecoilSpeed * Time.deltaTime);
        }

        private void ResetDirectionalRecoil()
        {
            CurrentDirectionalRecoil = Vector3.MoveTowards(CurrentDirectionalRecoil, Vector3.zero, definition.ResetDirectionalRecoilSpeed * Time.deltaTime);
            _targetDirectionalRecoil = Vector3.MoveTowards(_targetDirectionalRecoil, Vector3.zero, definition.ResetDirectionalRecoilSpeed * Time.deltaTime);
        }

        #endregion
    }
}
