using MoreSlugcats;
using RWCustom;
using UnityEngine;

namespace Hailstorm;

public class IceCrystal : Weapon
{
    // Spear variables

    private bool spinning;
    private int stillCounter;

    private int stuckBodyPart;
    private int stuckInChunkIndex;
    public float stuckRotation;
    public BodyChunk StuckInChunk => stuckInObject.bodyChunks[stuckInChunkIndex];
    public PhysicalObject stuckInObject;
    public Appendage.Pos stuckInAppendage;

    public float Damage = 1.34f;
    public float Chill = 20;

    //-------------------------------
    public override int DefaultCollLayer => 1;
    public override bool HeavyWeapon => true;

    private bool lastModeThrown;

    public LightSource light;

    public float beingEaten;

    public float swallowed;

    public bool shatterTimerGo;
    public int shatterTimer;

    //-------------------------------------

    public int crystalType;
    public Color color2;

    //----------------------------------------------------------------------------------
    //----------------------------------------------------------------------------------

    int lizCrystalType = -1;
    Color lizColor1;
    Color lizColor2;

    public IceCrystal(AbstractIceCrystal absCrs, World world) : base(absCrs, world)
    {
        bodyChunks = new BodyChunk[1];
        bodyChunks[0] = new BodyChunk(this, 0, new Vector2(0f, 0f), 6f, 0.12f);
        bodyChunkConnections = new BodyChunkConnection[0];

        airFriction = 1f;
        gravity = 0.9f;
        bounce = 0.66f;
        surfaceFriction = 0.05f;
        collisionLayer = 1;        
        waterFriction = 1f;
        buoyancy = 1f;       
        stuckBodyPart = -1;
        firstChunk.loudness = 10f;
        soundLoop = new ChunkDynamicSoundLoop(firstChunk);
        //--------------------------------------------------
        Random.State state = Random.state;
        Random.InitState(absCrs.ID.RandomSeed);

        if (absCrs.lizCrystalType != -1 && absCrs.baseColor != Color.clear && absCrs.accentColor != Color.clear)
        {
            lizCrystalType = absCrs.lizCrystalType;
            lizColor1 = absCrs.baseColor;
            lizColor2 = absCrs.accentColor;
        }

        crystalType =
            (lizCrystalType != -1)? lizCrystalType : Random.Range(0, 6);

        color =
            (lizColor1 != Color.clear)? lizColor1 : Custom.HSL2RGB(Custom.WrappedRandomVariation(220f / 360f, 40 / 360f, 0.35f), 0.60f, Custom.ClampedRandomVariation(0.75f, 0.05f, 0.2f));

        if (lizColor2 != Color.clear)
        {
            color2 = lizColor2;
        }
        else
        {
            Color.RGBToHSV(color, out float h, out float s, out float v);
            h *= (h * 1.2272f > 0.75f) ? 0.7728f : 1.2272f;
            v -= 0.1f;
            color2 = Color.HSVToRGB(h, s, v);
        }       

        Random.state = state;
        //--------------------------------------------------
    }

    public override void Update(bool eu)
    {
        if (shatterTimer >= 100)
        {
            Destroy();
        }

        // Grants each created Ice Crystal a soft glow.
        if (light == null)
        {
            light = new LightSource(firstChunk.pos, environmentalLight: false, color, this);
            light.affectedByPaletteDarkness = 0.25f;
            room.AddObject(light);
        }
        else
        {
            light.setPos = firstChunk.pos;
            light.setRad = 80f;
            light.setAlpha = 0.5f;
            if (light.slatedForDeletetion || light.room != room)
            {
                light = null;
            }
        }

        if (firstChunk.vel.magnitude > 5f)
        {
            if (mode == Mode.Thrown)
                soundLoop.sound = SoundID.Spear_Thrown_Through_Air_LOOP;

            else if (mode == Mode.Free)
                soundLoop.sound = SoundID.Spear_Spinning_Through_Air_LOOP;
            
            soundLoop.Volume = Mathf.InverseLerp(5f, 15f, firstChunk.vel.magnitude);
        }
        soundLoop.Update();
        if (mode == Mode.Free)
        {
            if (spinning)
            {
                if (Custom.DistLess(firstChunk.pos, firstChunk.lastPos, room.gravity * 4f)) stillCounter++;
                else stillCounter = 0;

                if (firstChunk.ContactPoint.y < 0 || stillCounter > 20)
                {
                    spinning = false;
                    rotationSpeed = 0f;
                    firstChunk.vel *= 0f;
                    room.PlaySound(SoundID.Spear_Stick_In_Ground, firstChunk);
                }
            }
            else if (!Custom.DistLess(firstChunk.lastPos, firstChunk.pos, 12))
            { 
                SetRandomSpin(); 
            }
        }
        else if (mode == Mode.Thrown) { firstChunk.vel.y += 0.45f; }
        else if (mode == Mode.StuckInCreature)
        {
            if (stuckInAppendage != null)
            {
                setRotation = Custom.DegToVec(stuckRotation + Custom.VecToDeg(stuckInAppendage.appendage.OnAppendageDirection(stuckInAppendage)));
                firstChunk.pos = stuckInAppendage.appendage.OnAppendagePosition(stuckInAppendage);
            }
            else
            {
                Creature ctr = StuckInChunk.owner as Creature;
                firstChunk.vel = StuckInChunk.vel;
                if (stuckBodyPart == -1 || !room.BeingViewed || ctr.BodyPartByIndex(stuckBodyPart) == null)
                {
                    setRotation = Custom.DegToVec(stuckRotation + Custom.VecToDeg(StuckInChunk.Rotation));
                    firstChunk.MoveWithOtherObject(eu, StuckInChunk, new Vector2(0f, 0f));
                }
                else
                {
                    setRotation = Custom.DegToVec(stuckRotation + Custom.AimFromOneVectorToAnother(StuckInChunk.pos, ctr.BodyPartByIndex(stuckBodyPart).pos));
                    firstChunk.MoveWithOtherObject(eu, StuckInChunk, Vector2.Lerp(StuckInChunk.pos, ctr.BodyPartByIndex(stuckBodyPart).pos, 0.5f) - StuckInChunk.pos);
                }
            }
        }

        for (int num = abstractPhysicalObject.stuckObjects.Count - 1; num >= 0; num--)
        {
            AbstractPhysicalObject.AbstractObjectStick stuckie = abstractPhysicalObject.stuckObjects[num];
            if (stuckie is AbstractPhysicalObject.ImpaledOnSpearStick)
            {
                if (stuckie.B.realizedObject != null && (stuckie.B.realizedObject.slatedForDeletetion || stuckie.B.realizedObject.grabbedBy.Count > 0))
                {
                    stuckie.Deactivate();
                }
                else if (stuckie.B.realizedObject != null && stuckie.B.realizedObject.room == room)
                {
                    stuckie.B.realizedObject.firstChunk.MoveFromOutsideMyUpdate(eu, firstChunk.pos + rotation * Custom.LerpMap((abstractPhysicalObject.stuckObjects[num] as AbstractPhysicalObject.ImpaledOnSpearStick).onSpearPosition, 0f, 4f, 15f, -15f));
                    stuckie.B.realizedObject.firstChunk.vel *= 0f;
                }
            }
        }
        lastModeThrown = mode == Mode.Thrown;
        if (lastModeThrown && (firstChunk.ContactPoint.x != 0 || firstChunk.ContactPoint.y != 0))
        {
            Explode();
        }


        if (firstChunk.ContactPoint.y != 0)
        {
            rotationSpeed = (rotationSpeed * 2f + firstChunk.vel.x * 5f) / 3f;
        }

        bool grabbed = false;
        if (mode == Mode.Carried && grabbedBy.Count > 0 && grabbedBy[0].grabber is Player plr && plr.swallowAndRegurgitateCounter > 50 && plr.objectInStomach == null && plr.input[0].pckp)
        {
            int grasp = -1;
            for (int k = 0; k < 2; k++)
            {
                if (plr.grasps[k] != null && plr.CanBeSwallowed(plr.grasps[k].grabbed))
                {
                    grasp = k;
                    break;
                }
            }
            if (grasp > -1 && plr.grasps[grasp] != null && plr.grasps[grasp].grabbed == this)
            {
                grabbed = true;
            }
        }
        swallowed = Custom.LerpAndTick(swallowed, grabbed ? 1f : 0f, 0.05f, 0.05f);

        if (shatterTimerGo) shatterTimer++;
        if (shatterTimer > Random.Range(20, 40)) Explode();

        base.Update(eu);
    }

    public override void PickedUp(Creature upPicker)
    {
        ChangeMode(Mode.Carried);
        room.PlaySound(SoundID.Slugcat_Pick_Up_Spear, firstChunk);
    }
    public override void Thrown(Creature thrownBy, Vector2 thrownPos, Vector2? firstFrameTraceFromPos, IntVector2 throwDir, float frc, bool eu)
    {
        base.Thrown(thrownBy, thrownPos, firstFrameTraceFromPos, throwDir, frc, eu);        
        room?.PlaySound(SoundID.Slugcat_Throw_Spear, firstChunk);
        if (thrownBy is Player thrower)
        {
            firstChunk.pos.y -= 3;
            if (thrower.SlugCatClass == HailstormSlugcats.Incandescent)
            {
                if (!MMF.cfgUpwardsSpearThrow.Value || setRotation.Value.y != 1f || thrower.bodyMode == Player.BodyModeIndex.ZeroG)
                {
                    Damage *= 0.75f;
                }
                firstChunk.vel.x *= 0.8f;
            }
            else if (thrower.slugcatStats.throwingSkill == 0)
            {
                throwModeFrames = 18;
                Damage *= 0.6f + 0.3f * Mathf.Pow(Random.value, 4f);
                firstChunk.vel.x *= 0.77f;
            }
            else if (MMF.cfgUpwardsSpearThrow.Value && setRotation.Value.y == 1f && thrower.bodyMode != Player.BodyModeIndex.ZeroG)
            {
                Damage *= 0.8f;
            }
            else if (thrower.slugcatStats.throwingSkill == 2)
            {
                if (thrower.SlugCatClass != MoreSlugcatsEnums.SlugcatStatsName.Gourmand)
                {
                    Damage *= 1.25f;
                }
                else
                {                                 
                    if (thrower.gourmandExhausted)
                    {
                        Damage *= 0.3f;
                    }
                    else
                    {
                        Damage *= 3f;
                        if (thrower.canJump != 0) thrower.animation = Player.AnimationIndex.Roll;
                        else thrower.animation = Player.AnimationIndex.Flip;

                        if ((room != null && room.gravity == 0f) || Mathf.Abs(thrower.firstChunk.vel.x) < 1f)
                        {
                            thrower.firstChunk.vel += firstChunk.vel.normalized * 9f;
                        }
                        else
                        {
                            thrower.rollDirection = (int)Mathf.Sign(firstChunk.vel.x);
                            thrower.rollCounter = 0;
                            thrower.firstChunk.vel.x += Mathf.Sign(firstChunk.vel.x) * 9f;
                        }
                        thrower.aerobicLevel = 1f;
                        thrower.gourmandAttackNegateTime = 80;
                    }
                }
                firstChunk.vel.x *= 1.2f;
            }
        }
    }

    public override void TerrainImpact(int chunk, IntVector2 direction, float speed, bool firstContact)
    {
        base.TerrainImpact(chunk, direction, speed, firstContact);
        if (speed > 5 && firstContact)
        {
            int sparkAmount = (int)(speed / 5);
            for (int i = 0; i < sparkAmount; i++)
            {
                room.AddObject(new Spark(new Vector2(firstChunk.pos.x, room.MiddleOfTile(firstChunk.pos).y + firstChunk.ContactPoint.y * 10f), firstChunk.vel * Random.value + Custom.RNV() * Random.value * 4f - firstChunk.ContactPoint.ToVector2() * 4f * Random.value, new Color(1f, 1f, 1f), null, 6, 18));
            }
        }
        if (speed > 20f && firstContact)
        {
            Explode();
            Vector2 pos = bodyChunks[chunk].pos + direction.ToVector2() * bodyChunks[chunk].rad * 0.9f;
            for (int i = 0; i < Mathf.Round(Custom.LerpMap(speed, 5f, 15f, 2f, 8f)); i++)
            {
                //room.AddObject(new Spark(pos, direction.ToVector2() * Custom.LerpMap(speed, 5f, 15f, -2f, -8f) + Custom.RNV() * Random.value * Custom.LerpMap(speed, 5f, 15f, 2f, 4f), Color.Lerp(new Color(1f, 0.2f, 0f), new Color(1f, 1f, 1f), Random.value * 0.5f), null, 19, 47));
            }
        }
    }
    public override void HitWall()
    {
        Explode();
        SetRandomSpin();
        ChangeMode(Mode.Free);
        forbiddenToPlayer = 10;
    }
    public override void HitByWeapon(Weapon weapon)
    {
        base.HitByWeapon(weapon);
        Explode();
    }
    public override void HitByExplosion(float hitFac, Explosion explosion, int hitChunk)
    {
        base.HitByExplosion(hitFac, explosion, hitChunk);
        Explode();
    }    
    public override bool HitSomething(SharedPhysics.CollisionResult target, bool eu)
    {
        // Stops running this method if nothing was actually hit.
        if (target.obj == null) return false;                 

        // Checks for if a hit is valid for granting the player points. 
        bool arenaPointsOnHit = false;
        if (abstractPhysicalObject.world.game.IsArenaSession && abstractPhysicalObject.world.game.GetArenaGameSession.GameTypeSetup.spearHitScore != 0 && thrownBy != null && thrownBy is Player && target.obj is Creature ctr)
        {
            arenaPointsOnHit = true;
            if ((ctr.State is HealthState ctrHP && ctrHP.health <= 0f) || (ctr.State is not HealthState && ctr.dead))
            {
                arenaPointsOnHit = false;
            }
        }

        if (target.obj is Creature victim)
        {
            if (!CWT.CreatureData.TryGetValue(victim, out CreatureInfo cI) || victim is null) return false;

            if (victim is not Player || victim.SpearStick(this, Mathf.Lerp(0.55f, 0.62f, Random.value), target.chunk, target.onAppendagePos, firstChunk.vel))
            {
                float ColdRes = victim.Template.damageRestistances[HailstormEnums.ColdDamage.index, 0];

                if (victim.Template.type == MoreSlugcatsEnums.CreatureTemplateType.FireBug && !victim.dead) victim.Die();

                // Implements Gourmand's chance to survive a spear attack for this weapon.
                if (victim is Player gourm && gourm.SlugCatClass == MoreSlugcatsEnums.SlugcatStatsName.Gourmand && Random.value < 0.15f)
                {
                    Damage /= 2f;
                    Chill /= 2;
                    if (RainWorld.ShowLogs) Debug.Log("GOURMAND SAVE!");
                }

                // Sets final damage and Hypothermia gain.
                if (victim is Player && CWT.PlayerData.TryGetValue(victim as Player, out HailstormSlugcats hS))
                {
                    Damage *= hS.ColdDMGmult;
                    cI.chillTimer += (int)(Chill * hS.ColdDMGmult);
                }
                else if (ColdRes != 1)
                {
                    cI.chillTimer += (int)(Chill / ColdRes);
                }

                /* Deals damage and stun. 
                 * The Violence function can't actually deal non-whole-number damage to Slugcats, although it still stuns them just fine.
                 * To remedy this, permanentDamageTracking was made to act as effective health for players, which must be adjusted manually as seen below. */
                victim.Violence(firstChunk, firstChunk.vel * firstChunk.mass * 2f, target.chunk, target.onAppendagePos, HailstormEnums.ColdDamage, Damage, 40 * Damage);               
                if (victim is Player self)
                {
                    self.playerState.permanentDamageTracking += Damage / self.Template.baseDamageResistance;
                    if (self.playerState.permanentDamageTracking >= 1) self.Die();
                }
                ChangeMode(Mode.StuckInCreature);               
            }
        }
        else if (target.chunk != null)
        {
            target.chunk.vel += firstChunk.vel * firstChunk.mass / target.chunk.mass;
        }
        else if (target.onAppendagePos != null)
        {
            (target.obj as IHaveAppendages).ApplyForceOnAppendage(target.onAppendagePos, firstChunk.vel * firstChunk.mass);
        }

        if (target.chunk != null)
        {
            Color lerpedColor = Color.Lerp( Color.Lerp(color, color2, Random.Range(0f, 1f)), Color.white, Random.Range(0f, 1f));

            room.AddObject(new ExplosionSpikes(room, target.chunk.pos + Custom.DirVec(target.chunk.pos, target.collisionPoint) * target.chunk.rad, Random.Range(4, 6), 1.4f, Random.Range(10, 18), 10f, 30f, lerpedColor));
        }

        if (target.obj is Creature creature && creature.SpearStick(this, Mathf.Lerp(0.55f, 0.62f, Random.value), target.chunk, target.onAppendagePos, firstChunk.vel))
        {
            shatterTimerGo = true;
            shatterTimer += 10;
            room.PlaySound(SoundID.Spear_Stick_In_Creature, firstChunk);
            LodgeInCreature(target, eu);            
            if (arenaPointsOnHit) abstractPhysicalObject.world.game.GetArenaGameSession.PlayerLandSpear(thrownBy as Player, stuckInObject as Creature);
            return true;
        }

        shatterTimerGo = true;       
        room.PlaySound(SoundID.Spear_Bounce_Off_Creauture_Shell, firstChunk);
        ChangeMode(Mode.Free);
        vibrate = 20;
        firstChunk.vel = firstChunk.vel * -0.5f + Custom.DegToVec(Random.value * 360f) * Mathf.Lerp(0.1f, 0.4f, Random.value) * firstChunk.vel.magnitude;
        SetRandomSpin();
        return false;
    }
    public override void HitSomethingWithoutStopping(PhysicalObject obj, BodyChunk chunk, Appendage appendage)
    {
        if (obj is Creature ctr)
        {
            if (thrownBy != null)
            {
                ctr.SetKillTag(thrownBy.abstractCreature);
            }
            ctr.Die();                        
        }
        base.HitSomethingWithoutStopping(obj, chunk, appendage);
    }

    public void ProvideRotationBodyPart(BodyChunk chunk, BodyPart bodyPart)
    {       
        stuckBodyPart = bodyPart.bodyPartArrayIndex;
        stuckRotation = Custom.Angle(firstChunk.vel, (bodyPart.pos - chunk.pos).normalized);
        bodyPart.vel += firstChunk.vel;
    }
    private void LodgeInCreature(SharedPhysics.CollisionResult target, bool eu)
    {       
        stuckInObject = target.obj;
        AbstractCreature victim = (target.obj as Creature).abstractCreature;
        
        if (target.chunk != null)
        {
            stuckInChunkIndex = target.chunk.index;
            if (stuckBodyPart == -1)
            {
                stuckRotation = Custom.Angle(throwDir.ToVector2(), StuckInChunk.Rotation);
            }
            firstChunk.MoveWithOtherObject(eu, StuckInChunk, new Vector2(0f, 0f));
            Debug.Log("Add ice crystal to creature chunk " + StuckInChunk.index);
            new AbstractPhysicalObject.AbstractSpearStick(abstractPhysicalObject, victim, stuckInChunkIndex, stuckBodyPart, stuckRotation);
        }
        else if (target.onAppendagePos != null)
        {
            stuckInChunkIndex = 0;
            stuckInAppendage = target.onAppendagePos;
            stuckRotation = Custom.VecToDeg(rotation) - Custom.VecToDeg(stuckInAppendage.appendage.OnAppendageDirection(stuckInAppendage));
            Debug.Log("Add ice crystal to creature Appendage");
            new AbstractPhysicalObject.AbstractSpearAppendageStick(abstractPhysicalObject, victim, target.onAppendagePos.appendage.appIndex, target.onAppendagePos.prevSegment, target.onAppendagePos.distanceToNext, stuckRotation);
        }
        if (room.BeingViewed)
        {
            for (int i = 0; i < 8; i++)
            {
                room.AddObject(new WaterDrip(target.collisionPoint, -firstChunk.vel * Random.value * 0.5f + Custom.DegToVec(360f * Random.value) * firstChunk.vel.magnitude * Random.value * 0.5f, waterColor: false));
            }
        }
    }
    public override void RecreateSticksFromAbstract()
    {
        for (int i = 0; i < abstractPhysicalObject.stuckObjects.Count; i++)
        {
            if (abstractPhysicalObject.stuckObjects[i] is AbstractPhysicalObject.AbstractSpearStick spearStick && spearStick.Spear == abstractPhysicalObject && (abstractPhysicalObject.stuckObjects[i] as AbstractPhysicalObject.AbstractSpearStick).LodgedIn.realizedObject != null)
            {
                AbstractPhysicalObject.AbstractSpearStick abstractSpearStick = abstractPhysicalObject.stuckObjects[i] as AbstractPhysicalObject.AbstractSpearStick;
                stuckInObject = abstractSpearStick.LodgedIn.realizedObject;
                stuckInChunkIndex = abstractSpearStick.chunk;
                stuckBodyPart = abstractSpearStick.bodyPart;
                stuckRotation = abstractSpearStick.angle;
                ChangeMode(Mode.StuckInCreature);
            }
            else if (abstractPhysicalObject.stuckObjects[i] is AbstractPhysicalObject.AbstractSpearAppendageStick spearAppenStick && spearAppenStick.Spear == abstractPhysicalObject && (abstractPhysicalObject.stuckObjects[i] as AbstractPhysicalObject.AbstractSpearAppendageStick).LodgedIn.realizedObject != null)
            {
                AbstractPhysicalObject.AbstractSpearAppendageStick abstractSpearAppendageStick = abstractPhysicalObject.stuckObjects[i] as AbstractPhysicalObject.AbstractSpearAppendageStick;
                stuckInObject = abstractSpearAppendageStick.LodgedIn.realizedObject;
                stuckInAppendage = new Appendage.Pos(stuckInObject.appendages[abstractSpearAppendageStick.appendage], abstractSpearAppendageStick.prevSeg, abstractSpearAppendageStick.distanceToNext);
                stuckRotation = abstractSpearAppendageStick.angle;
                ChangeMode(Mode.StuckInCreature);
            }
        }
    }
    public virtual void TryImpaleSmallCreature(Creature smallCrit)
    {
        int num = 0;
        int num2 = 0;
        for (int i = 0; i < abstractPhysicalObject.stuckObjects.Count; i++)
        {
            if (abstractPhysicalObject.stuckObjects[i] is AbstractPhysicalObject.ImpaledOnSpearStick impaled)
            {
                if (impaled.onSpearPosition == num2)
                {
                    num2++;
                }
                num++;
            }
        }
        if (num <= 5 && num2 < 5)
        {
            new AbstractPhysicalObject.ImpaledOnSpearStick(abstractPhysicalObject, smallCrit.abstractCreature, 0, num2);
        }    
    }
    public override void ChangeMode(Mode newMode)
    {
        base.ChangeMode(newMode);

        if (mode == Mode.StuckInCreature)
        {
            if (room != null)
            {
                room.PlaySound(SoundID.Spear_Dislodged_From_Creature, firstChunk);
            }
            PulledOutOfStuckObject();
            ChangeOverlap(newOverlap: true);
        }
        else if (newMode == Mode.StuckInCreature)
        {
            ChangeOverlap(newOverlap: false);
            collisionLayer = 2;
        }

        if (newMode != Mode.Thrown)
        {
            Damage = 1.34f;
        }    
    }
    public void PulledOutOfStuckObject()
    {
        for (int i = 0; i < abstractPhysicalObject.stuckObjects.Count; i++)
        {
            if (abstractPhysicalObject.stuckObjects[i] is AbstractPhysicalObject.AbstractSpearStick spearStick && spearStick.Spear == abstractPhysicalObject)
            {
                abstractPhysicalObject.stuckObjects[i].Deactivate();
                break;
            }
            if (abstractPhysicalObject.stuckObjects[i] is AbstractPhysicalObject.AbstractSpearAppendageStick spearAppenStick && spearAppenStick.Spear == abstractPhysicalObject)
            {
                abstractPhysicalObject.stuckObjects[i].Deactivate();
                break;
            }
        }
        stuckInObject = null;
        stuckInAppendage = null;
        stuckInChunkIndex = 0;
    }

    //---------------------------------

    public void Explode()
    {
        if (slatedForDeletetion)
        {
            return;
        }

        InsectCoordinator smallInsects = null;
        for (int i = 0; i < room.updateList.Count; i++)
        {
            if (room.updateList[i] is InsectCoordinator)
            {
                smallInsects = room.updateList[i] as InsectCoordinator;
                break;
            }
        }

        // Adds particle effects for the crystal being smashed.
        for (int j = 0; j < 12; j++)
        {
            if (j % 2 == 1)
            {
                room.AddObject(new HailstormSnowflake(firstChunk.pos, Custom.RNV() * Random.value * 12f, color, color2));
                room.AddObject(new FreezerMist(firstChunk.pos, Custom.RNV() * Random.value * 10f, color, color2, 0.2f, thrownBy?.abstractCreature, smallInsects, false));
            }           
            else
            {
                room.AddObject(new PuffBallSkin(firstChunk.pos, Custom.RNV() * Random.value * 12f, color, color2));
            }
        }
        room.AddObject(new FreezerMistVisionObscurer(firstChunk.pos));
        room.PlaySound(SoundID.Coral_Circuit_Break, firstChunk.pos, 1.25f, 1.5f);

        shatterTimer = 100;
    }

    //----------------------------------------------------------------------------------
    //----------------------------------------------------------------------------------

    public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        sLeaser.sprites = new FSprite[2];

        sLeaser.sprites[0] = new FSprite("IceCrystal" + crystalType + "B");
        sLeaser.sprites[1] = new FSprite("IceCrystal" + crystalType + "A");

        AddToContainer(sLeaser, rCam, null);
    }

    public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        Vector2 vector = Vector2.Lerp(firstChunk.lastPos, firstChunk.pos, timeStacker);
        Vector3 vector2 = Vector3.Slerp(lastRotation, rotation, timeStacker);      

        if (vibrate > 0 && mode != Mode.Carried)
        {
            vector += Custom.DegToVec(Random.value * 360f) * 2f * Random.value;
        }
        float swallowShrink = 1f;
        if (beingEaten > 0f || swallowed > 0f)
        {
            swallowShrink = 1f - Mathf.Max(beingEaten, swallowed * 0.5f);
        }

        // Sets the basic display information for every sprite this object uses.
        for (int i = 0; i < 2; i++)
        {
            sLeaser.sprites[i].x = vector.x - camPos.x;
            sLeaser.sprites[i].y = vector.y - camPos.y;
            sLeaser.sprites[i].rotation = Custom.AimFromOneVectorToAnother(new Vector2(0f, 0f), vector2);
            sLeaser.sprites[i].scale = 0.66f * swallowShrink;
        }

        if (blink > 0)
        {
            bool pickupPrompt = blink > 1 && Random.value < 0.5f;
            sLeaser.sprites[0].color = (pickupPrompt ? blinkColor : color2);
            sLeaser.sprites[1].color = (pickupPrompt ? blinkColor : color);
        }
        else
        {            
            sLeaser.sprites[0].color = color2;
            sLeaser.sprites[1].color = color;
        }

        if (slatedForDeletetion || room != rCam.room)
        {
            sLeaser.CleanSpritesAndRemove();
        }
    }

    public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
    {
        //color = Color.Lerp(new Color(129f/255f, 200f/255f, 236f/255f), palette.texture.GetPixel(11, 4), 0.5f); // THIS IS HOW THE GAME PULLS COLORS FROM PALETTES! IMPORTANT!!!!!!
    }

    public override void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContainer)
    {
        foreach (FSprite fsprite in sLeaser.sprites)
        {
            fsprite.RemoveFromContainer();           
            rCam.ReturnFContainer("Items").AddChild(fsprite);
        }
    }
}


//---------------------------------------------------------------------
//---------------------------------------------------------------------

// Snowflake particles. This code is adapted from the code used for PuffballSkin.
public class HailstormSnowflake : CosmeticSprite
{
    public Vector2 rotation;

    public Vector2 lastRotation;

    public Vector2 randomDir;

    private Color color;

    private Color color2;

    public float lastLife;

    public float life;

    public float lifeTime;

    public float randomRotat;

    public float flip;

    public float lastFlip;

    public float flipDir;

    public HailstormSnowflake(Vector2 pos, Vector2 vel, Color color, Color color2)
    {
        lastPos = pos;
        base.vel = vel;
        this.color = color;
        this.color2 = color2;
        base.pos = pos + vel.normalized * 60f * Random.value;
        rotation = Custom.RNV();
        lastRotation = Custom.RNV();
        randomDir = Custom.RNV();
        lastLife = 1f;
        life = 1f;
        lifeTime = Mathf.Lerp(140f, 160f, Random.value);
        randomRotat = Random.value * 360f;
        flipDir = ((Random.value < 0.5f) ? (-1f) : 1f);
    }

    public override void Update(bool eu)
    {
        vel *= Custom.LerpMap(vel.magnitude, 1f, 10f, 0.999f, 0.8f);
        vel.y -= 0.1f;
        vel += randomDir * 0.17f;
        Vector2 val = randomDir + Custom.RNV() * 0.8f;
        randomDir = vel.normalized;
        lastRotation = rotation;
        val = lastRotation + Custom.DirVec(pos, lastPos) * 0.3f;
        rotation = vel.normalized;
        lastFlip = flip;
        flip += flipDir / Mathf.Lerp(6f, 80f, Random.value);
        lastLife = life;
        life -= 1f / lifeTime;
        if (room.GetTile(pos).Solid)
        {
            life -= 0.05f;
        }
        if (lastLife <= 0f)
        {
            Destroy();
        }
        base.Update(eu);
    }

    public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        sLeaser.sprites = new FSprite[2];
        for (int i = 0; i < 2; i++)
        {
            sLeaser.sprites[i] = new FSprite("Snowflake" + Random.Range(0, 1) + ".0");
            sLeaser.sprites[i].scaleX = ((Random.value < 0.5f)? -1f : 1f) * Mathf.Lerp(0.5f, 1f, Random.value);
            sLeaser.sprites[i].scaleY = ((Random.value < 0.5f)? -1f : 1f) * 1.2f;
        }
        AddToContainer(sLeaser, rCam, null);
    }

    public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        Vector2 val = Vector2.Lerp(lastPos, pos, timeStacker);
        sLeaser.sprites[0].x = val.x - camPos.x;
        sLeaser.sprites[0].y = val.y - camPos.y;
        sLeaser.sprites[0].rotation = Custom.VecToDeg(Vector3.Slerp(lastRotation, rotation, timeStacker)) + randomRotat;
        sLeaser.sprites[0].scale = 0.7f * Mathf.InverseLerp(0f, 0.3f, Mathf.Lerp(lastLife, life, timeStacker));
        float num = Mathf.Sin(Mathf.Lerp(lastFlip, flip, timeStacker) * Mathf.PI * 2f);
        sLeaser.sprites[0].scaleX = num;
        sLeaser.sprites[0].color = ((num > 0f) ? color : color2);
        base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
    }

    public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
    {
    }

    // Try seeing if you can get away with deleting AddToContainer and ApplyPalette. Though the latter might be useful.
    public override void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
    {
        base.AddToContainer(sLeaser, rCam, newContatiner);
    }
}

//------------------------------------------------------------------------------------------------------------------------------------------