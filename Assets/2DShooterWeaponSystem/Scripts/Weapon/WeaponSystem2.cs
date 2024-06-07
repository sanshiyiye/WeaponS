using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Random = UnityEngine.Random;
using UnityEngine.InputSystem;

/// <summary>
/// The meat of the asset, the WeaponSystem component is used to hold weapon configurations, and bullet pattern setups as well as handling the logic for firing the bullets and setting bullet properties up based on the configuration.
/// </summary>
public class WeaponSystem2 : MonoBehaviour
{
    #region Properties


    /// <summary>
    /// The name assigned to the weapon configuration that is currently equipped.
    /// </summary>
    public string weaponName;

    /// <summary>
    /// The spread that bullets will use for their pattern (when more than one bullet)
    /// </summary>
    [Range(0f, 10f)] public float bulletSpread;

    /// <summary>
    /// The maximum spread to bounce up to when ping pong spread is enabled.
    /// </summary>
    [Range(0f, 10f)]
    public float bulletSpreadPingPongMax;

    /// <summary>
    /// The minimum spread to bounce down to when ping pong spread is enabled.
    /// </summary>
    [Range(0f, 10f)]
    public float bulletSpreadPingPongMin;

    /// <summary>
    /// The speed at which the bullet spread is bounced between.
    /// </summary>
    [Range(1f, 3f)]
    public float spreadPingPongSpeed = 1f;

    /// <summary>
    /// The spacing between bullets when using more than one bullet.
    /// </summary>
    [Range(0f, 5f)]
    public float bulletSpacing;

    /// <summary>
    /// The number of bullets to fire.
    /// </summary>
    [Range(0, 20)]
    public int bulletCount;

    /// <summary>
    /// An amount of randomness to add to bullets when firing them (adds jitter to bullet streams / patterns).
    /// </summary>
    [Range(0f, 5f)]
    public float bulletRandomness;

    /// <summary>
    /// Speed at which bullets travel
    /// </summary>
    [Range(0f, 35f)]
    public float bulletSpeed;

    /// <summary>
    /// Bullet sine wave oscillation factor
    /// </summary>
    [Range(0f, 50f)]
    public float bulletAmplitude;

    [Range(1,50f)]
    public float bulletCircleRadius;
    
    /// <summary>
    /// Bullet frequency increment. Added each frame when bulletAmplitude is used.
    /// </summary>
    [Range(0.01f, 1f)]
    public float bulletFrequencyIncrement;

    /// <summary>
    /// 
    /// </summary>
    [Range(0f, 3f)]
    public float weaponFireRate;

    /// <summary>
    /// Offset on the X to position the start of a bullet when fired from a 'gunPoint' transform.
    /// </summary>
    [Range(-3f, 3f)]
    public float weaponXOffset;

    /// <summary>
    /// Offset on the Y to position the start of a bullet when fired from a 'gunPoint' transform.
    /// </summary>
    [Range(-3f, 3f)]
    public float weaponYOffset;

    /// <summary>
    /// The chance a bullet fired from this weapon will have to bounce / ricochet off other colliders.
    /// </summary>
    [Range(0, 100f)]
    public float ricochetChancePercent;

    

    // /// <summary>
    // /// The starting ammo for a weapon configuration.
    // /// </summary>
    // public int startingAmmo;

    // /// <summary>
    // /// Amount of ammo used in a weapon configuration thus far.
    // /// </summary>
    // public int ammoUsed;
    

    /// <summary>
    /// The target for the weapon to track in 'Turret Track' mode.
    /// </summary>
	public Transform targetToTrack;

    // /// <summary>
    // /// The rate at which a weapon in 'Turret Track' mode will turn to face it's designated target.
    // /// </summary>
    // [Range(1f, 15f)]
    // public float trackingTurnRate;

    // /// <summary>
    // /// If enabled, then the turn rate of a weapon system in 'Turret Track' mode whilst tracking its assigned target will be eased (instead of the weapon always instantly facing it's target).
    // /// </summary>
    // public bool lerpTurnRate;

    /// <summary>
    /// If you are using plain white sprites this color value can be used to dynamically change bullet color in-game.
    /// </summary>
    public Color bulletColour = Color.white;

    /// <summary>
    /// The texture2D assigned to a weapon configuration - can be used for GUI display of selected weapon for example.
    /// </summary>
    public Texture2D weaponIcon;

    // /// <summary>
    // /// Enable this to automatically fire without need for input (useful for enemies etc)
    // /// </summary>
    // public bool autoFire;

    /// <summary>
    /// Enable to bounce the spread value of bullets between the pingpong spread min and max values (causes bullet pattern to 'swing' up and down).
    /// </summary>
    public bool pingPongSpread;

    /// <summary>
    /// When enabled, this will rotate the gun point transform around 360 degrees at a constant rate, allowing you to form circular bullet patterns. Requires weapon system to be set to GunPoint relative mode.
    /// </summary>
    public bool circularFireMode;

    /// <summary>
    /// The rotations per minute that the gunPoint transform will rotate with, if circularFireMode is enabled.
    /// </summary>
    public float circularFireRotationRate = 240f;

    // /// <summary>
    // /// Enable if bullets fired from the weapon should allow ricochets (based on percent chance for bullets to ricochet).
    // /// </summary>
    // public bool richochetsEnabled;

    /// <summary>
    /// Enable if bullets from this weapon should spawn hit effects (like sparks for example) when impacting colliders.
    /// </summary>
    public bool hitEffectEnabled;
    

    // /// <summary>
    // /// Enabled if the weapon is configured to have ammo and use magazines.
    // /// </summary>
    // public bool usesMagazines;

    // /// <summary>
    // /// Used to track if a weapon configuration is the first equip or not.
    // /// </summary>
    // public bool isFirstEquip;
    

    // /// <summary>
    // /// If enabled, the aiming/facing direction is calculated relative to the transform that the WeaponSystem component is placed on. Offset X and Y positioning of bullets is taken into account and applied in this mode.
    // /// </summary>
    // public bool weaponRelativeToComponent;

    // /// <summary>
    // /// Duplicates the number of bullets being used and mirrors the extra bullets on the X-axis
    // /// </summary>
    // public bool mirrorX;
    

    /// <summary>
    /// The aim angle the weapon system is currently at in radians.
    /// </summary>
    public float aimAngle;
    
    /// <summary>
    /// The shot fired SFX clip for the selected weapon configuration
    /// </summary>
    public AudioClip shotFiredClip;

    /// <summary>
    /// The gun point or transform location that bullets are shot / generated from when shooting. This is used if the WeaponSystem configuration is set to relative to gun point mode.
    /// </summary>
    public Transform gunPoint;

    /// <summary>
    /// Boolean value to toggle shooting
    /// </summary>
    public bool isShooting;
    
    public BulletOption bulletOptionType;
    
    public ShooterType shooterDirectionType;

        

    #endregion
    /// <summary>
    /// Default weapon presets. Feel free to add new ones here! Just don't forget to handle the new bullet presets in the BulletPresetChangedHandler method that is fired with the BulletPresetChanged event.
    /// </summary>


    public delegate void BulletPresetChangeHandler();

    /// <summary>
    /// Event that fires when a bullet preset is changed.
    /// </summary>
    public event BulletPresetChangeHandler BulletPresetChanged;

    public delegate void WeaponConfigurationChangedHandler(Transform gunPointTransform, int weaponConfigIndex);

    /// <summary>
    /// Event that fires when a Weapon Configuration is changed (weapon changed).
    /// </summary>
    public event WeaponConfigurationChangedHandler WeaponConfigurationChanged;

    public delegate void WeaponShootingStartedHandler();

    public delegate void WeaponShootingFinishedHandler();

    /// <summary>
    /// Event that fires when shooting starts.
    /// </summary>
    public event WeaponShootingStartedHandler WeaponShootingStarted;

    /// <summary>
    /// Event that fires when shooting stops.
    /// </summary>
    public event WeaponShootingFinishedHandler WeaponShootingFinished;

    /// <summary>
    /// (Legacy - pre version 1.3, but still usable) This property dictates what weapon is selected for the WeaponSystem. Set it to any BulletPresetType Enum value and the event should take care of setting up the weapon for you.
    /// Just ensure that if you add a new Enum value to BulletPresetType, that you create an entry for it in the switch statement in the BulletPresetChangedHandler.
    /// All you need is a reference to your WeaponSystem script, once you have that, you can change you weapon selection from anywhere in your game using this.
    /// </summary>
    public BulletPresetType BulletPreset
    {
        get
        {
            return _bulletPreset;
        }
        set
        {
            if (BulletPreset == value) return;

            // Set a few defaults back whenever the weapon is changed
            pingPongSpread = false;
            weaponXOffset = 0.74f;
            weaponYOffset = 0f;
            gunPoint.transform.localPosition = new Vector2(0.6f, 0f);

            // Set the property and fire off the event if subscribed to.
            _bulletPreset = value;
            if (BulletPresetChanged != null) BulletPresetChanged();
        }
    }

    /// <summary>
    /// The list of weapon configurations on this Weapon System.
    /// </summary>
    public List<WeaponSystemConfig> weaponConfigs
    {
        get
        {
            return _weaponConfigs;
        }
        set
        {
            _weaponConfigs = value;
        }
    }

    /// <summary>
    /// The upper index of all weapon configurations - used to determine when to roll over to the first weapon again when switching configurations up.
    /// </summary>
    public int MaxWeaponIndex
    {
        get;
        set;
    }


    [SerializeField]
    private List<WeaponSystemConfig> _weaponConfigs;

    /// <summary>
    /// Tracking for which bullet preset type is currently selected (legacy / pre version 1.3 used).
    /// </summary>
    private BulletPresetType _bulletPreset;

    /// <summary>
    /// Used to track current cool down status of bullets firing
    /// </summary>
    private float coolDown;

    /// <summary>
    /// Tracks initial spread value of bullets
    /// </summary>
    private float bulletSpreadInitial;

    /// <summary>
    /// The initial value used for spacing bullets
    /// </summary>
    private float bulletSpacingInitial;

    /// <summary>
    /// Value that holds the spread increment of bullets - changes based on how many bullets are being used in bulletCount and the bulletSpread value.
    /// </summary>
    private float bulletSpreadIncrement;

    /// <summary>
    /// Determines how bullets are spaced out based on their spacing value and the number of bullets being used.
    /// </summary>
    private float bulletSpacingIncrement;

    /// <summary>
    /// The currently selected weapon configuration
    /// </summary>
    private int selectedWeaponIndex;

    // /// <summary>
    // /// Flag to determine if weapon configuration is currently reloading or not.
    // /// </summary>
    // private bool isReloading;

    public WeaponSystemConfig weaponConfig;

    private void Awake()
    {
        var input = GetComponent<PlayerInput>();
        if (input != null)
        {
            input.onActionTriggered += HandleActions;
        }
        
        transform.eulerAngles = new Vector3(0.0f, 0.0f, 2 * Mathf.Rad2Deg);
    }

    // Use this for initialization
    void Start()
    {

        WeaponConfigurationChanged += WeaponSystemWeaponConfigurationChanged;

        // Set a default bullet colour, otherwise bullets will be invisible.
        bulletColour = Color.white;

        // Subscribe to the BulletPresetChanged Event.
        BulletPresetChanged += BulletPresetChangedHandler;

        MaxWeaponIndex = weaponConfigs.Count - 1;
        EquipWeaponConfiguration(0); // Equip the first weapon configuration if possible.
    }

    private void HandleActions(InputAction.CallbackContext context)
    {
        switch (context.action.name)
        {
            case "Shoot":
                if (context.phase == InputActionPhase.Performed)
                {
                    isShooting = true;
                }
                else
                {
                    isShooting = false;
                }
                break;

            case "SwitchWeapon":
                if (context.phase == InputActionPhase.Performed)
                {
                    float v = (float)context.ReadValueAsObject();
                    if (v > 0f)
                    {
                        // Return if we don't have any configurations setup.
                        if (_weaponConfigs.Count <= 0) return;

                        // Save our current weapon's used ammo property as well as magazine usage if applicable. We also store whether the weapon has been equipped for the first time or not
                        // StoreCurrentWeaponIndexAmmoUsedValue();
                        // StoreCurrentWeaponIndexMagazineUsage();
                        // StoreFirstEquippedStatus();

                        if (selectedWeaponIndex < MaxWeaponIndex)
                        {
                            selectedWeaponIndex = (selectedWeaponIndex + 1);
                        }
                        else
                        {
                            // rollover to the first weapon index
                            selectedWeaponIndex = 0;
                        }

                        // Equip the selected weapon configuration
                        EquipWeaponConfiguration(selectedWeaponIndex);
                    }
                    else if (v < 0f)
                    {
                        // Return if we don't have any configurations setup.
                        if (_weaponConfigs.Count <= 0) return;

                        // Save our current weapon's used ammo property, as well as magazine usage if applicable. We also store whether the weapon has been equipped for the first time or not
                        // StoreCurrentWeaponIndexAmmoUsedValue();
                        // StoreCurrentWeaponIndexMagazineUsage();
                        // StoreFirstEquippedStatus();

                        if (selectedWeaponIndex >= 1)
                        {
                            selectedWeaponIndex = (selectedWeaponIndex - 1);
                        }
                        else
                        {
                            // rollback to the last weapon index
                            selectedWeaponIndex = MaxWeaponIndex;
                        }

                        // Equip the selected weapon configuration
                        EquipWeaponConfiguration(selectedWeaponIndex);
                    }
                }
                break;
        }
    }

    /// <summary>
    /// Fires when weapon is changed.
    /// </summary>
    /// <param name="gunPointTransform">The transform being used as the 'gunpoint' or 'shoot from' point.</param>
    /// <param name="weaponConfigIndex">The weapon configuration 'slot' index to equip.</param>
    void WeaponSystemWeaponConfigurationChanged(Transform gunPointTransform, int weaponConfigIndex)
    {
        gunPoint.transform.position = gunPointTransform.position;
    }
    

    /// <summary>
    /// From version 1.3 onward, you can setup weapon "configurations" which are stored in the weaponConfigs List property. You should use this
    /// EquipWeaponConfiguration method to change weapons, passing in the index of the list for the weapon you want to select. Use the in-editor inspector on the WeaponSystem component to setup new configurations and
    /// add them to the inventory/configurations list.
    /// </summary>
    /// <param name="slot">The weapon configuration 'slot' index to equip.</param>
    public void EquipWeaponConfiguration(int slot)
    {
        if (_weaponConfigs.Count <= 0) return;
        var weaponConfig = _weaponConfigs[slot];
        if (weaponConfig == null) return; // Return if an invalid weaponConfig is requested

        var config = _weaponConfigs[slot];
        weaponName = config.WeaponName;
        
        bulletColour = config.BulletColour;
        bulletCount = config.BulletCount;
        bulletRandomness = config.BulletRandomness;
        bulletSpacing = config.BulletSpacing;
        bulletSpeed = config.BulletSpeed;
        bulletAmplitude = config.BulletAmplitude;
        // circle
        bulletCircleRadius = config.BulletCircleRadius;
        
        bulletFrequencyIncrement = config.BulletFrequencyIncrement;
        bulletSpread = config.BulletSpread;
        bulletSpreadPingPongMax = config.BulletSpreadPingPongMax;
        bulletSpreadPingPongMin = config.BulletSpreadPingPongMin;
        spreadPingPongSpeed = config.SpreadPingPongSpeed;
        weaponFireRate = config.WeaponFireRate;
        weaponXOffset = config.WeaponXOffset;
        weaponYOffset = config.WeaponYOffset;
        ricochetChancePercent = config.RicochetChancePercent;
        // startingAmmo = config.AmmoAvailable;
       
        pingPongSpread = config.PingPongSpread;
        // richochetsEnabled = config.RichochetsEnabled;
        hitEffectEnabled = config.HitEffectEnabled;
        
        weaponIcon = config.WeaponIcon;
        // weaponRelativeToComponent = config.WeaponRelativeToComponent;
        
       
        // isFirstEquip = config.IsFirstEquip;

        shotFiredClip = config.ShotFiredClip;
        targetToTrack = config.TargetToTrack;
        // trackingTurnRate = config.TrackingTurnRate;
        // lerpTurnRate = config.LerpTurnRate;
        // mirrorX = config.MirrorX;
        circularFireMode = config.CircularFireMode;
        circularFireRotationRate = config.CircularFireRotationRate;

        bulletOptionType = config.BulletOptionType;
        shooterDirectionType = config.ShooterType;

        // If the weapon uses magazines we want to setup initial magazine clip with bullets etc the first time the weapon is equipped...
        // if ( isFirstEquip )
        // {
        //     isFirstEquip = false;
        // }

        if (WeaponConfigurationChanged != null) WeaponConfigurationChanged(gunPoint, slot);
    }
    

    /// <summary>
    /// (Legacy - pre version 1.3 but still usable). This method should be subscribed to the BulletPresetChanged Event. This event fires whenever the BulletPreset Enum property value changes.
    /// Set up any new BulletPresetType Enum weapon types in here. When you set the public Enum property, this method should fire, and the case statement relevant to your selection should run.
    /// Note: this is the older method of changing weapons. From version 1.3 onward, you can setup weapon "configurations" which are stored in the weaponConfigs List property. You should use the
    /// EquipWeaponConfiguration method to changing weapons, passing in the index of the list for the weapon you want to select. Use the in-editor inspector on the WeaponSystem component to setup new configurations and
    /// add them to the inventory/configurations list.
    /// </summary>
    private void BulletPresetChangedHandler()
    {
        switch (BulletPreset)
        {
            case BulletPresetType.Simple:
                bulletCount = 1;
                weaponFireRate = 0.15f;
                bulletSpacing = 1f;
                bulletSpread = 0.05f;
                bulletSpeed = 12f;
                bulletRandomness = 0.15f;
                break;

            case BulletPresetType.GatlingGun:
                bulletCount = 3;
                weaponFireRate = 0.05f;
                bulletSpacing = 0.25f;
                bulletSpread = 0.0f;
                bulletSpeed = 20f;
                bulletRandomness = 0.35f;
                break;

            case BulletPresetType.Shotgun:
                bulletCount = 8;
                weaponFireRate = 0.5f;
                bulletSpacing = 0.5f;
                bulletSpread = 0.65f;
                bulletSpeed = 15f;
                bulletRandomness = 0.65f;
                break;

            case BulletPresetType.WildFire:
                bulletCount = 4;
                weaponFireRate = 0.06f;
                bulletSpacing = 0.13f;
                bulletSpread = 0.24f;
                bulletSpeed = 15f;
                bulletRandomness = 1f;
                break;

            case BulletPresetType.Tarantula:
                bulletSpreadPingPongMin = 1.5f;
                bulletSpreadPingPongMax = 4f;
                spreadPingPongSpeed = 2.5f;
                pingPongSpread = true;
                bulletCount = 8;
                weaponFireRate = 0.063f;
                bulletSpacing = 0.53f;
                bulletSpread = 0.08f;
                bulletSpeed = 7.35f;
                bulletRandomness = 0.0f;
                break;

            case BulletPresetType.CrazySpreadPingPong:
                bulletSpreadPingPongMax = 1f;
                spreadPingPongSpeed = 2.5f;
                pingPongSpread = true;
                bulletCount = 7;
                weaponFireRate = 0.0f;
                bulletSpacing = 0.08f;
                bulletSpread = 0.08f;
                bulletSpeed = 19.35f;
                bulletRandomness = 0.08f;
                break;

            case BulletPresetType.DualSpread:
                bulletCount = 2;
                weaponFireRate = 0.15f;
                bulletSpacing = 0.1f;
                bulletSpread = 0.3f;
                bulletSpeed = 13f;
                bulletRandomness = 0.01f;
                break;

            case BulletPresetType.ThreeShotSpread:
                bulletCount = 3;
                weaponFireRate = 0.15f;
                bulletSpacing = 0.1f;
                bulletSpread = 0.3f;
                bulletSpeed = 13f;
                bulletRandomness = 0.01f;
                break;

            case BulletPresetType.ImbaCannon:
                bulletCount = 10;
                weaponFireRate = 0.02f;
                bulletSpacing = 0.05f;
                bulletSpread = 0.08f;
                bulletSpeed = 25f;
                bulletRandomness = 0.23f;
                break;

            case BulletPresetType.Shower:
                bulletCount = 9;
                weaponFireRate = 0.02f;
                bulletSpacing = 0.05f;
                bulletSpread = 0.7f;
                bulletSpeed = 21.8f;
                bulletRandomness = 0.19f;
                break;

            case BulletPresetType.DualAlternating:
                bulletSpreadPingPongMax = 1f;
                spreadPingPongSpeed = 2f;
                pingPongSpread = true;
                bulletCount = 2;
                weaponFireRate = 0.05f;
                bulletSpacing = 0.24f;
                bulletSpread = 0.08f;
                bulletSpeed = 14.5f;
                bulletRandomness = 0.0f;
                break;

            case BulletPresetType.DualMachineGun:
                bulletCount = 2;
                weaponFireRate = 0.07f;
                bulletSpacing = 0.53f;
                bulletSpread = 0.011f;
                bulletSpeed = 16f;
                bulletRandomness = 0.02f;
                break;

            case BulletPresetType.CircleSpray:
                weaponXOffset = 0f;
                weaponYOffset = 0f;
                bulletCount = 20;
                weaponFireRate = 0.19f;
                bulletSpacing = 0f;
                bulletSpread = 5f;
                bulletSpeed = 5f;
                bulletRandomness = 0f;
                break;

            default:
                bulletCount = 1;
                weaponFireRate = 0.15f;
                bulletSpacing = 1f;
                bulletSpread = 0.05f;
                bulletSpeed = 12f;
                bulletRandomness = 0.15f;
                break;
        }

        // mirrorX = false;
    }


    /// <summary>
    /// Processing of bullets to fire is done here, from ammo usage to angle, number, properties, and color of bullets.
    /// </summary>
    private void ProcessBullets()
    {


        if (bulletCount > 1)
        {
            bulletSpreadInitial = -bulletSpread / 2;
            bulletSpacingInitial = bulletSpacing / 2;
            bulletSpreadIncrement = bulletSpread / (bulletCount - 1);
            bulletSpacingIncrement = bulletSpacing / (bulletCount - 1);
        }
        else
        {
            bulletSpreadInitial = 0f;
            bulletSpacingInitial = 0f;
            bulletSpreadIncrement = 0f;
            bulletSpacingIncrement = 0f;
        }

        //Shooting audio
        if (shotFiredClip != null) AudioSource.PlayClipAtPoint(shotFiredClip, Vector2.zero);

        // For each 'gun' attachment the player has we'll setup each bullet accordingly...
        for (var i = 0; i < bulletCount; i++)
        {
            

            var bullet = GetBulletFromPool();
            var bulletComponent = (Bullet)bullet.GetComponent(typeof(Bullet));

            var offsetX = Mathf.Cos(aimAngle - Mathf.PI / 2) * (bulletSpacingInitial - i * bulletSpacingIncrement);
            var offsetY = Mathf.Sin(aimAngle - Mathf.PI / 2) * (bulletSpacingInitial - i * bulletSpacingIncrement);

            // if (circularFireMode)
            // {
            //     bulletComponent.directionAngle = (gunPoint.eulerAngles.z * Mathf.Deg2Rad) + bulletSpreadInitial + i * bulletSpreadIncrement;
            // }
            // else
            // {
                bulletComponent.DirectionAngle = aimAngle + bulletSpreadInitial + i * bulletSpreadIncrement;
            // }

            bulletComponent.speed = bulletSpeed;
            bulletComponent.circularFireMode = circularFireMode;
            bulletComponent.amplitude = bulletAmplitude;
            bulletComponent.frequencyIncrement = bulletFrequencyIncrement;

            // Setup the point at which bullets need to be placed based on all the parameters
            var initialPosition = gunPoint.position + (gunPoint.transform.forward * (bulletSpacingInitial - i * bulletSpacingIncrement));
            var bulletPosition = new Vector3(initialPosition.x + offsetX + Random.Range(0f, 1f) * bulletRandomness - bulletRandomness / 2,
                initialPosition.y + offsetY + Random.Range(0f, 1f) * bulletRandomness - bulletRandomness / 2, initialPosition.z);

            bullet.transform.position = bulletPosition;

            // bulletComponent.BulletXPosition = bullet.transform.position.x;
            // bulletComponent.BulletYPosition = bullet.transform.position.y;
            bulletComponent.SetBulletPosition(bullet.transform.position.x, bullet.transform.position.y);

            // Initial chance to ricochet as the bullet comes out. If the bullet bounces again, this will be determined on the next bullet collision.
            bulletComponent.ricochetChancePercent = ricochetChancePercent;
            bulletComponent.canRichochet = bulletComponent.GetRicochetChance();
            bulletComponent.useHitEffects = hitEffectEnabled;

            // Activate the bullet to get it going
            bullet.SetActive(true);

            // Set the bullet colour using the renderer that we cached when the bullet was first created at the start of the scene. This is easy and accurate if we have a white/semi-transparent bullet sprite
            if (bulletComponent.bulletSpriteRenderer != null)
            {
                if (bulletComponent.bulletSpriteRenderer is SpriteRenderer)
                {
                    ((SpriteRenderer)bulletComponent.bulletSpriteRenderer).color = bulletColour;
                }
                

            }

            
        }
    }

    /// <summary>
    /// Returns the flipped angle of the input angle in radians.
    /// </summary>
    /// <param name="aimAngle"></param>
    /// <returns>The flipped angle</returns>
    private float GetFlippedAngle(float inputAngle)
    {
        float flipAimAngle;
        if (inputAngle >= 0)
        {
            flipAimAngle = Mathf.PI - inputAngle;
        }
        else
        {
            flipAimAngle = -(Mathf.PI) - inputAngle;
        }

        return flipAimAngle;
    }

    /// <summary>
    /// Fetch a bullet from the ObjectPoolManager instance depending on what BulletOption type enum is selected for the selected weapon configuration. The pool will generate a new bullet if there are no available bullets to pull from the pool.
    /// </summary>
    /// <returns></returns>
    private GameObject GetBulletFromPool()
    {
        GameObject bullet;

        // Pull a bullet object from our pool based on current bullet Enum selection.
        switch (bulletOptionType)
        {
            case BulletOption.Spherical:
                bullet = ObjectPoolManager.instance.GetUsableSphereBullet();
                break;

            case BulletOption.TracerHorizontal:
                bullet = ObjectPoolManager.instance.GetUsableStandardHorizontalBullet();
                break;

            case BulletOption.TurretHorizontal:
                bullet = ObjectPoolManager.instance.GetUsableTurretBullet();
                break;

            case BulletOption.Beam1:
                bullet = ObjectPoolManager.instance.GetUsableBeam1Bullet();
                break;

            case BulletOption.Beam2:
                bullet = ObjectPoolManager.instance.GetUsableBeam2Bullet();
                break;

            case BulletOption.Beam3:
                bullet = ObjectPoolManager.instance.GetUsableBeam3Bullet();
                break;

            case BulletOption.Beam4:
                bullet = ObjectPoolManager.instance.GetUsableBeam4Bullet();
                break;

            case BulletOption.Beam5:
                bullet = ObjectPoolManager.instance.GetUsableBeam5Bullet();
                break;

            case BulletOption.Beam6:
                bullet = ObjectPoolManager.instance.GetUsableBeam6Bullet();
                break;

            case BulletOption.Laser:
                bullet = ObjectPoolManager.instance.GetUsableLaserBullet();
                break;
            default:
                bullet = ObjectPoolManager.instance.GetUsableSphereBullet();
                break;
        }

        return bullet;
    }
    

    // Update is called once per frame
    void Update()
    {
        var facingDirection = Vector2.zero;

        // We have three modes - horizontal, vertical and free. Each is a different case based on a dropdown Enum selector on the WeaponSystem script.
        // Horizontal is like a sideways scrolling horizontal shooter, vertical is the same, but with the player locked facing upward instead of right, and free allows the player to turn in any direction based on the mouse position.
        switch (shooterDirectionType)
        {

            case ShooterType.FreeAim:

                Gamepad gamepad = Gamepad.current;
                if (gamepad == null)
                {
                    // Get the world position of the mouse cursor and set facing direction to that minus the player's current position.
                    var worldMousePosition = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0f));
                    // Calculate based on whether this weapon configuration is set relative to the WeaponSystem object, or the assigned gunpoint object.
                    facingDirection = worldMousePosition - transform.position;
                    CalculateAimAndFacingAngles(facingDirection);
                }
                
                break;

            case ShooterType.TargetTracking:

                // Face the specified target to track.
                // Calculate based on whether this weapon configuration is set relative to the WeaponSystem object, or the assigned gunpoint object.
                facingDirection = targetToTrack.position - transform.position;
                CalculateAimAndFacingAngles(facingDirection);
                break;

            default:
                // Default the player to face horizontally right if no selection is made
                facingDirection = Vector2.right;
                CalculateAimAndFacingAngles(facingDirection);
                break;
        }

        HandleShooting();

        if (pingPongSpread)
        {
            bulletSpread = Mathf.PingPong(Time.time * spreadPingPongSpeed, bulletSpreadPingPongMax - bulletSpreadPingPongMin) + bulletSpreadPingPongMin;
        }

        // if (circularFireMode)
        // {
        //     gunPoint.Rotate(0, 0, circularFireRotationRate * Time.deltaTime, Space.Self);
        // }

        // Set the weapon offset position - the gunpoint transform needs to be a child of the player gameobject's transform, if in "relative to object" mode.
        gunPoint.transform.localPosition = new Vector3(weaponXOffset, weaponYOffset, gunPoint.transform.localPosition.z);
    }

    /// <summary>
    /// Flags the selected weapon as the first equipped / starting weapon configuration - generally only called at the instantiation of a WeaponSystem component.
    /// </summary>
    // private void StoreFirstEquippedStatus()
    // {
    //     if (_weaponConfigs[selectedWeaponIndex] != null)
    //         _weaponConfigs[selectedWeaponIndex].IsFirstEquip = isFirstEquip;
    // }
    

    private void ShootWithCoolDown()
    {
        if (coolDown <= 0f)
        {
            ProcessBullets();
            coolDown = weaponFireRate;
        }



    }

    /// <summary>
    /// Change weapon cooldown timer down so it gets closer to being ready to fire again based on cooldown time and handle player shooting controls input
    /// </summary>
    public void HandleShooting()
    {
        coolDown -= Time.deltaTime;
        if (isShooting && shooterDirectionType != ShooterType.TargetTracking)
        {
            
            if (WeaponShootingStarted != null) WeaponShootingStarted();
            ShootWithCoolDown();
        }
        else
        {
            if (WeaponShootingFinished != null) WeaponShootingFinished();
            isShooting = false;
        }
    }

    /// <summary>
    /// Calculate aim angle and other settings that apply to all ShooterType orientations
    /// </summary>
    /// <param name="facingDirection"></param>
    private void CalculateAimAndFacingAngles(Vector2 facingDirection)
    {

            aimAngle = Mathf.Atan2(facingDirection.y, facingDirection.x);
            if (aimAngle < 0f)
            {
                aimAngle = Mathf.PI * 2 + aimAngle;
            }

            transform.eulerAngles = new Vector3(0.0f, 0.0f, aimAngle * Mathf.Rad2Deg);
    }
}