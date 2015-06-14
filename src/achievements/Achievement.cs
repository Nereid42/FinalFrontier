using System;
using UnityEngine;
using Contracts;

namespace Nereid
{
   namespace FinalFrontier
   {
      public abstract class Achievement : Activity, IComparable<Achievement>
      {
         private bool first;
         public int prestige { get; private set; }

         protected Achievement(String code, String name,int prestige, bool first)
            : base(code, (first ? "First " : "") + name)
         {
            this.first = first;
            this.prestige = prestige;
         }

         public virtual bool Check(VesselState previous, VesselState current) { return false; }
         public virtual bool Check(HallOfFameEntry entry) { return false; }
         public virtual bool Check(EventReport report) { return false; }
         public virtual bool Check(Contract contract) { return false; }

         // description of the achievement
         public abstract String GetText();

         public virtual void ChangeEntryOnAward(HallOfFameEntry entry)
         {
            // should be overriden and implemented by subclasses
         }

         // output line in logbook
         public override String CreateLogBookEntry(LogbookEntry entry)
         {
            return GetName() + " ribbon is awarded to " + entry.Name;
         }

         public bool HasToBeFirst()
         {
            return first;
         }

         protected String FirstKerbalText()
         {
            return first ? "being first kerbal" : "";
         }

         public int CompareTo(Achievement right)
         {
            if (prestige < right.prestige) return 1;
            if (prestige > right.prestige) return -1;
            if (!first && right.first) return 1;
            if (first && !right.first) return -1;
            return 0;
         }

      }

      abstract class NumericAchievement : Achievement
      {
         protected int value;
         public NumericAchievement(String code, String name, int value, int prestige, bool first)
            : base(code+value, name, prestige, first)
         {
            this.value = value;
         }

      }

      class DangerousEvaAchievement : Achievement
      {
         public DangerousEvaAchievement(int prestige)
            : base("DE", "Dangerous EVA", prestige, false)
         {
         }

         public override bool Check(VesselState previous, VesselState current)
         {
            if (current == null) return false;
            // only an EVA could be a dangerous EVA
            if (!current.IsEVA) return false;
            // vessel changes wont count
            if (previous == null) return false;
            // EVA starts from a previous non EVA; no new EVA, no danger
            if (previous.IsEVA) return false;
            // EVA from a vessel in orbit isn't dangerous
            if (previous.InOrbit) return false;
            // EVA in orbit isnt dangerous
            if (current.InOrbit) return false;
            // surface is safe, too
            if (previous.IsLanded) return false;
            // surface is safe, too
            if (current.IsLanded) return false;
            // a splashdown EVA is not really dangerous
            if( !previous.IsEVA && previous.Situation == Vessel.Situations.SPLASHED) return false;
            if (current.Situation == Vessel.Situations.SPLASHED )  return false;
            // danger ahead
            return true;
         }

         public override String GetText()
         {
            return "Awarded for executing EVA while not in a stable orbit";
         }
      }

      class SplashdownAchievement : Achievement
      {
         public SplashdownAchievement(int prestige)
            : base("W", "Splashdown", prestige, false)
         {
         }

         public override bool Check(VesselState previous, VesselState current)
         {
            if (current == null) return false;
            // no eva (we dont accept a bathing)
            if (current.IsEVA) return false;
            // only flying vessels can do a splashdown (no rovers on shores can do a splashdown)
            if (previous != null && previous.Situation != Vessel.Situations.FLYING) return false;
            // are we splashed?
            return current.Situation==Vessel.Situations.SPLASHED;
         }

         public override String GetText()
         {
            return "Awarded for a splashdown of a vessel in water";
         }
      }


      class FastOrbitAchievement : NumericAchievement
      {
         public FastOrbitAchievement(int secondsToOrbit, int prestige)
            : base("FO:", "Fast Orbit", secondsToOrbit, prestige, false)
         {
         }

         public override bool Check(VesselState previous, VesselState current)
         {
            if (current == null) return false;
            // we need a previous vessel state  
            if (previous == null) return false;
            // only new orbits count
            if (!current.InOrbit || previous.InOrbit) return false;
            // only orbits around kerbin are valid
            if (!current.MainBody.isHomeWorld) return false;
            // mission took too long...
            if (current.MissionTime >= value) return false;
            // check for real Kerbin launch time
            if (current.Origin == null) return false;
            double now = Planetarium.GetUniversalTime();
            double launchTime = VesselObserver.Instance().GetHomeworldLaunchTime(current.Origin);
            if (launchTime < 0 || now - launchTime >= value) return false;
            // orbit established around kerbin in less than <value> seconds
            return true;
         }         

         public override String GetText()
         {
            return "Awarded for less than " + value + " seconds into orbit";
         }
      }

      class MissionTimeAchievement : NumericAchievement
      {
         public MissionTimeAchievement(int durationInSeconds, int prestige)
            : base("MT:", "Mission Time", durationInSeconds, prestige, false)
         {
         }

         public override bool Check(HallOfFameEntry entry)
         {
            return entry.TotalMissionTime > value;
         }

         public override String GetText()
         {
            return "Awarded for more than " + Utils.GameTimeInDays(value) + (GameUtils.IsKerbinTimeEnabled()?" kerbin":"")+" days spent in missions";
         }
      }

      class SingleMissionTimeAchievement : NumericAchievement
      {
         public SingleMissionTimeAchievement(int durationInSeconds, int prestige)
            : base("ME:", "Endurance", durationInSeconds, prestige, false)
         {
         }

         public override bool Check(HallOfFameEntry entry)
         {
            // get kerbal
            ProtoCrewMember kerbal = entry.GetKerbal();
            // if kerbal is still inflight, he has not yet returned ("has to return safely") 
            if (kerbal.InCrewOfActiveFlight()) return false; 
            // get vessel
            Vessel vessel = kerbal.GetVessel();
            // no vessel, no mission time, no mission, sorry
            if (vessel == null) return false;
            // check the mission time...
            return vessel.missionTime > value;
         }

         public override String GetText()
         {
            return "Awarded for more than " + Utils.GameTimeInDays(value) + (GameUtils.IsKerbinTimeEnabled() ? " kerbin" : "") + " days spent in a single mission and returnig safely";
         }
      }

      class EvaTimeAchievement : NumericAchievement
      {
         public EvaTimeAchievement(int durationInSeconds, int prestige)
            : base("EM:", "EVA Endurance", durationInSeconds, prestige, false)
         {
         }

         public override bool Check(HallOfFameEntry entry)
         {
            // no recorded start time of EVA, no EVA
            if (entry.TimeOfLastEva <= 0) return false;
            // check duration of last EVA
            return entry.LastEvaDuration >= value;
         }

         public override String GetText()
         {
            return "Awarded for continuously spending " + Utils.GameTimeAsString(value) + " in EVA";
         }
      }

      class EvaTotalTimeAchievement : NumericAchievement
      {
         public EvaTotalTimeAchievement(int durationInSeconds, int prestige)
            : base("ET:", "EVA Time", durationInSeconds, prestige, false)
         {
         }

         public override bool Check(HallOfFameEntry entry)
         {
            return entry.TotalEvaTime > value;
         }

         public override String GetText()
         {
            return "Awarded for more than " + Utils.GameTimeAsString(value) + " spent in EVA";
         }
      }

      class MissionsFlownAchievement : NumericAchievement
      {
         public MissionsFlownAchievement(int missionsFlown, int prestige)
            : base("M:", "Multiple Missions", missionsFlown, prestige, false)
         {
         }

         public override bool Check(HallOfFameEntry entry)
         {
            return entry.MissionsFlown >= value;
         }

         public override String GetText()
         {
            return "Awarded for " + value + " or more missions";
         }
      }

      class HeavyVehicleAchievement : NumericAchievement
      {
         public HeavyVehicleAchievement(int mass, int prestige)
            : base("H:", "Heavy Vehicle", mass, prestige, false)
         {
         }

         public override bool Check(VesselState previous, VesselState current)
         {
            if (current == null) return false;
            // no EVA
            if (current.IsEVA) return false;
            // check mass
            if (current.Origin == null) return false;
            if (current.Origin.GetTotalMass() <= value) return false;
            return true;
         }   

         public override String GetText()
         {
            return "Awarded to every crew member of a vehicle with a total mass of " + value + "t or more";
         }
      }

      class HeavyVehicleLandAchievement : NumericAchievement
      {
         public HeavyVehicleLandAchievement(int mass, int prestige)
            : base("HS:", "Heavy Vehicle Landing", mass, prestige, false)
         {
         }

         public override bool Check(VesselState previous, VesselState current)
         {
            if (previous == null) return false;
            if (current == null) return false;
            // no EVA
            if (current.IsEVA) return false;
            // check mass
            if (current.Origin == null) return false;
            if (current.Origin.GetTotalMass() <= value) return false;
            // landed?
            if (current.Situation != Vessel.Situations.LANDED) return false;
            // situation has to change from non-landed to landed (
            if (!current.IsLanded || previous.IsLanded || previous.IsPrelaunch) return false;
            return true;
         }

         public override String GetText()
         {
            return "Awarded for landing a vehicle with a total mass of " + value + "t or more";
         }
      }


      class HeavyVehicleLaunchAchievement : NumericAchievement
      {
         public HeavyVehicleLaunchAchievement(int mass, int prestige)
            : base("HL:", "Heavy Vehicle Launch", mass, prestige, false)
         {
         }

         public override bool Check(VesselState previous, VesselState current)
         {
            if (current == null) return false; 
            if (previous == null) return false;
            // no EVA
            if (current.IsEVA) return false;
            // this has to be a launch
            if (previous.Situation != Vessel.Situations.LANDED && previous.Situation != Vessel.Situations.PRELAUNCH) return false;
            // if we are not flying this is not a launch
            if (current.Situation != Vessel.Situations.FLYING) return false;
            // check mass
            if (current.Origin == null) return false;
            if (current.Origin.GetTotalMass() <= value) return false;
            return true;
         }

         public override String GetText()
         {
            return "Awarded for launching a vehicle with a total mass of " + value + "t or more";
         }
      }


      /** not used yet */
      class MissionAbortedAchievement : Achievement
      {
         public MissionAbortedAchievement(int prestige)
            : base("MA","Mission Aborted",prestige, false)
         {
         }

         public override String GetText()
         {
            return "Awarded for surviving a mission abort";
         }
      }

      class InSpaceAchievement : Achievement
      {
         public InSpaceAchievement(int prestige)
            : base("S1", "Kerbal in Space", prestige, true)
         {
         }

         public override bool Check(VesselState previous, VesselState current)
         {
            if (current == null) return false;
            // no vessel change and no fresh EVA 
            if (previous == null) return false;
            if (current.Situation == Vessel.Situations.PRELAUNCH) return false;
            if (current.Situation == Vessel.Situations.LANDED) return false;
            // no main celestial body? we have to be deep in space then
            if (current.MainBody == null) return true;
            if (previous.altitude > current.MainBody.MaxAtmosphereAltitude()) return false;
            if (current.altitude <= current.MainBody.MaxAtmosphereAltitude()) return false;
            return true;
         }

         public override String GetText()
         {
            if (HasToBeFirst())
            {
               return "Awarded for being the first kerbal in space";
            }
            return "Awarded for leaving Kerbin atmosphere";
         }
      }

      class EnteringAtmosphereAchievement : CelestialBodyAchievement
      {
         public EnteringAtmosphereAchievement(CelestialBody body,int prestige, bool first = false)
            : base("A", body.GetName()+" Atmosphere", body, prestige, first)
         {
         }

         public override bool Check(VesselState previous, VesselState current)
         {
            if (current == null) return false;
            // no vessel change and no fesh EVA 
            if (previous == null) return false;
            // deep space, so no atmosphere
            if (current.MainBody == null) return false;
            // already in atmosphere?
            if (previous.IsInAtmosphere) return false;
            // still not in atmosphere
            if (!current.IsInAtmosphere) return false;
            // check main body
            return current.MainBody.Equals(body);
         }

         public override String GetText()
         {
            return "Awarded for" + FirstKerbalText().Envelope() + "entering the atmosphere of " + body.GetName();
         }

      }


      abstract class CelestialBodyAchievement : Achievement
      {
         protected CelestialBody body;

         public CelestialBodyAchievement(String code, String name, CelestialBody body, int prestige, bool first = false)
            : base(code+(first?"1:":":") + body.GetName(), name, prestige, first)
         {
            this.body = body;
         }


         public override void ChangeEntryOnAward(HallOfFameEntry entry)
         {
            // already visited, so no change
            if(entry.visitedCelestialBodies.Contains(this.body)) return;

            // Jool Tour
            if (!entry.joolTour)
            {
               CelestialBody jool = GameUtils.GetCelestialBody("Jool");
               if (jool != null)
               {
                  bool allVisited = true;
                  foreach (CelestialBody moon in jool.orbitingBodies)
                  {
                     if (!entry.visitedCelestialBodies.Contains(moon))
                     {
                        allVisited = false;
                        break;
                     }
                  }
                  if (allVisited)
                  {
                     entry.joolTour = true;
                     // award Jool tour ribbon
                     ProtoCrewMember kerbal = entry.GetKerbal();
                     Log.Detail("awarding jool tour ribbon to " + kerbal.name);
                     // try first Jool Tour if not already awarded (BROKEN)
                     //HallOfFame.Instance().Record(kerbal, RibbonPool.Instance().GetJoolTourRibbon(true));
                     // try non first Jool Tour 
                     HallOfFame.Instance().Record(kerbal, RibbonPool.Instance().GetJoolTourRibbon(false));
                  }

               }
            }


            // Grand Tour
           if(!entry.grandTour)
           {
              entry.visitedCelestialBodies.Add(this.body);
              // all celestial bodies visited?
              bool allVisited = true;
              foreach (CelestialBody body in PSystemManager.Instance.localBodies)
              {
                 if(!body.IsSun())
                 {
                    if(!entry.visitedCelestialBodies.Contains(body))
                    {
                       allVisited = false;
                       break;
                    }
                 }
              }
              if(allVisited)
              {
                 entry.grandTour = true;
                 // award grand tour ribbon
                 ProtoCrewMember kerbal = entry.GetKerbal();
                 Log.Detail("awarding grand tour ribbon to "+kerbal.name);
                 // try first Grand Tour if not already awarded (BROKEN)
                 //HallOfFame.Instance().Record(kerbal, RibbonPool.Instance().GetGrandTourRibbon(true));
                 // try non first Grand Tour 
                 HallOfFame.Instance().Record(kerbal, RibbonPool.Instance().GetGrandTourRibbon(false));
              }
           }
         }
      }

      class SphereOfInfluenceAchievement : CelestialBodyAchievement
      {

         public SphereOfInfluenceAchievement(CelestialBody body, int prestige)
            : base("I", body.GetName() + " Sphere of Influence", body, prestige, false)
         {
         }

         public override bool Check(VesselState previous, VesselState current)
         {
            if (current == null) return false; 
            if (!current.MainBody.Equals(body)) return false;
            if (previous == null || previous.MainBody.Equals(body)) return false;
            return true;
         }

         public override String GetText()
         {
            return "Awarded for entering the sphere of influence of " + body.GetName();
         }
      }

      class LandingAchievement : CelestialBodyAchievement
      {
         public LandingAchievement(CelestialBody body, int prestige, bool first = false)
            : base("L", "Landing on " + body.GetName(), body, prestige, first)
         {
         }

         public override bool Check(VesselState previous, VesselState current)
         {
            if (current == null) return false; 
            if (previous == null) return false;
            // Launching wont count
            if (previous.Situation == Vessel.Situations.PRELAUNCH) return false;
            //
            // EVA won't count
            if (current.IsEVA) return false;
            // are we on the correct planet/moon?
            if (!current.MainBody.Equals(body)) return false;
            // situation has to change from non-landed to landed
            if (!current.IsLanded || previous.IsLanded || previous.IsPrelaunch) return false;
            return true;
         }

         public override String GetText()
         {
            return "Awarded for" + FirstKerbalText().Envelope() + "landing on " + body.GetName();
         }
      }


      class PlantFlagAchievement : CelestialBodyAchievement
      {
         public PlantFlagAchievement(CelestialBody body, int prestige, bool first = false)
            : base("F", "Flag on " + body.GetName(), body, prestige, first)
         {
         }

         public override bool Check(VesselState previous, VesselState current)
         {
            if (current == null) return false;
            if (!current.HasFlagPlanted) return false;
            if (previous != null && previous.HasFlagPlanted) return false;
            if (!current.MainBody.Equals(body)) return false;
            return true;
         }

         public override String GetText()
         {
            return "Awarded for" + FirstKerbalText().Envelope() + "planting a flag on " + body.GetName();
         }
      }

      class EvaAchievement : CelestialBodyAchievement
      {
         private static readonly double NO_ATM = 0.0000001;

         public EvaAchievement(CelestialBody body, int prestige, bool first = false)
            : base("V", body.GetName() + " EVA", body, prestige, first)
         {
         }

         public override bool Check(VesselState previous, VesselState current)
         {
            if (previous != null) return false; // just check first EVA event
            if (current == null) return false;
            // no EVA, no achievement
            if (!current.IsEVA) return false;
            if (current.Situation == Vessel.Situations.LANDED) return false;
            if (current.Situation == Vessel.Situations.PRELAUNCH ) return false;
            if (current.Situation == Vessel.Situations.SPLASHED) return false;
            // no main celestial body? we have to be deep in space then
            if (current.MainBody == null) return true;
            if (current.atmDensity > NO_ATM) return false;
            if (current.altitude < current.MainBody.MaxAtmosphereAltitude()) return false;
            if (!current.MainBody.Equals(body)) return false;
            return true;
         }

         public override String GetText()
         {
            return "Awarded for" + FirstKerbalText().Envelope() + "on EVA in zero atmosphere around " + body.GetName();
         }
      }

      class FirstEvaInSpaceAchievement : Achievement
      {
         private static readonly double NO_ATM = 0.0000001;

         public FirstEvaInSpaceAchievement(int prestige)
            : base("V1", "EVA in Space", prestige, true)
         {
         }

         public override bool Check(VesselState previous, VesselState current)
         {
            if (previous != null) return false; // just check first EVA event
            if (current == null) return false;
            if (!current.IsEVA) return false;
            if (current.Situation == Vessel.Situations.LANDED) return false;
            if (current.Situation == Vessel.Situations.PRELAUNCH ) return false;
            if (current.Situation == Vessel.Situations.SPLASHED) return false;
            // no main celestial body? we have to be deep in space then
            if (current.MainBody == null) return true;
            if (current.atmDensity > NO_ATM) return false;
            if (current.altitude < current.MainBody.MaxAtmosphereAltitude()) return false;
            return true;
         }

         public override String GetText()
         {
            return "Awarded for being the first kerbal on EVA in space ";
         }
      }


      class EvaOrbitAchievement : CelestialBodyAchievement
      {
         private static readonly double NO_ATM = 0.0000001;

         public EvaOrbitAchievement(CelestialBody body, int prestige, bool first = false)
            : base("E", body.GetName() + " Orbital EVA", body, prestige, first)
         {
         }

         public override bool Check(VesselState previous, VesselState current)
         {
            if (current == null) return false; 
            if (previous != null && previous.IsEVA) return false; // just check first EVA event
            if (!current.IsEVA) return false;
            if (!current.MainBody.Equals(body)) return false;
            if (current.atmDensity > NO_ATM) return false;
            if (!current.InOrbit) return false;
            return true;
         }

         public override String GetText()
         {
            return "Awarded for" + FirstKerbalText().Envelope() + "on EVA in a stable orbit around " + body.GetName();
         }
      }


      class EvaGroundAchievement : CelestialBodyAchievement
      {
         public EvaGroundAchievement(CelestialBody body, int prestige, bool first = false)
            : base("G", body.GetName()+" Surface EVA", body, prestige, first)
         {
         }

         public override bool Check(VesselState previous, VesselState current)
         {
            if (current == null) return false; 
            if (previous != null && previous.IsEVA && previous.IsLanded) return false; // just check first EVA event that was no LANDING
            // no EVA no ribbon...
            if (!current.IsEVA) return false;
            // on the right place?
            if (!current.MainBody.Equals(body)) return false;
            // we have to be on ground
            if (!current.IsLanded) return false;
            // yeah, we got it
            return true;
         }

         public override String GetText()
         {
            return "Awarded for" + FirstKerbalText().Envelope() + "taking footsteps on " + body.GetName();
         }
      }

      abstract class OrbitalAchievement : CelestialBodyAchievement
      {
         public OrbitalAchievement(String code, String name, CelestialBody body, int prestige, bool first = false)
            : base(code, name, body, prestige, first)
         {
         }

         public override bool Check(VesselState previous, VesselState current)
         {
            if (current == null) return false; 
            return current.MainBody.Equals(body) && current.InOrbit;
         }
      }


      class OrbitAchievement : OrbitalAchievement
      {
         public OrbitAchievement(CelestialBody body, int prestige, bool first = false)
            : base("O", body.GetName()+" Orbit", body, prestige, first)
         {
         }

         public override bool Check(VesselState previous, VesselState current)
         {

            if (current == null) return false; 
            return current.MainBody.Equals(body) && current.InOrbit;
         }

         public override String GetText()
         {
            return "Awarded for" + FirstKerbalText().Envelope() + "orbiting around " + body.GetName();
         }
      }

      class RoverAchievement : CelestialBodyAchievement
      {

         public RoverAchievement(CelestialBody body, int prestige, bool first = false)
            : base("R", body.GetName() + " Rover Drive", body, prestige, first)
         {
         }

         public override bool Check(VesselState previous, VesselState current)
         {
            if (current == null) return false; 
            if (!current.MainBody.Equals(body)) return false;
            if (current.Origin == null) return false;
            if (current.Origin.vesselType != VesselType.Rover) return false;
            return current.movedOnSurface;
         }

         public override String GetText()
         {
            return "Awarded for" + FirstKerbalText().Envelope() + "moving a vehicle on surface of " + body.GetName();
         }
      }


      class CloserSolarOrbitAchievement : CelestialBodyAchievement
      {
         private double maxDistanceToSun;
         private CelestialBody innermost;

         public CloserSolarOrbitAchievement(CelestialBody body, int prestige, CelestialBody innermost, bool first = false)
            : base("CSO", "Closer Solar Orbit", body, prestige, first)
         {
            this.innermost = innermost;
            maxDistanceToSun = innermost.orbit.PeA / 2;
         }

         public override bool Check(VesselState previous, VesselState current)
         {
            if (current == null) return false; 
            if (!current.MainBody.Equals(body)) return false;
            return current.PeA <= maxDistanceToSun && current.ApA <= maxDistanceToSun;
         }

         public override String GetText()
         {
            return "Awarded for" + FirstKerbalText().Envelope() + "orbiting " + base.body.GetName() + " half between " + innermost.GetName() + " and " + base.body.GetName();
         }
      }


      class DockingAchievement : OrbitalAchievement
      {
         public DockingAchievement(CelestialBody body, int prestige, bool first = false)
            : base("DO", body.GetName()+" Docking", body, prestige, first)
         {
         }

         public override bool Check(VesselState previous, VesselState current)
         {
            if (current == null) return false; 
            if (previous != null && previous.Situation == Vessel.Situations.DOCKED) return false;
            if (current.Situation != Vessel.Situations.DOCKED) return false;
            return base.Check(previous, current);
         }

         public override String GetText()
         {
            return "Awarded for" + FirstKerbalText().Envelope() + "docking in " + body.GetName()+" orbit";
         }
      }

      class CustomAchievement : NumericAchievement
      {
         private String text;

         public CustomAchievement(int value, int prestige)
            : base("X","no name",value, prestige, false)
         {
         }

         public override String GetText()
         {
            if(text==null) return "no description";
            return text;
         }

         public void SetText(String text)
         {
            this.text = text;
         }

         public void SetName(String name)
         {
            base.Rename(name);
         }

         public int GetNr()
         {
            return value;
         }
      }

      class SolidFuelLaunchAchievement : NumericAchievement
      {
         public SolidFuelLaunchAchievement(int value, int prestige)
            : base("B", value+"%"+" Solid Fuel Booster ", value, prestige, false)
         {
         }

         public override bool Check(VesselState previous, VesselState current) 
         {
            if (current == null) return false;
            if (!current.IsLaunch) return false;
            Vessel vessel = current.Origin;
            if (vessel == null) return false;
            double amountOfActiveSolidFuel = 0;
            float totalMass = 0;
            foreach (Part part in vessel.parts)
            {
               totalMass += part.mass;
               if (part.State == PartStates.ACTIVE)
               {
                  foreach (PartResource resource in part.Resources)
                  {
                     if(resource.IsSolidFuel())
                     {
                        amountOfActiveSolidFuel += resource.amount;
                     }
                  }
               }
            }
            double ratio = amountOfActiveSolidFuel / totalMass / 1000;
            return ratio >= (double)value / 100.0; 
         }

         public override String GetText()
         {
            return "Awarded for launching with solid fuel booster at "+value+"% of ship mass";
         }

      }

      class HighGeeForceAchievement : NumericAchievement
      {
         public HighGeeForceAchievement(int value, int prestige)
            : base("H", "G-Force " + Utils.Roman(value), value, prestige, false)
         {
         }

         public override bool Check(VesselState previous, VesselState current)
         {
            if (current == null) return false;
            if (previous == null) return false;
            if (current.IsEVA) return false;
            if (current.Origin == null) return false;
            if (double.IsNaN(current.Origin.geeForce)) return false;
            if (current.Origin.geeForce  < value)  return false;
            if (VesselObserver.Instance().GetGeeForceSustained(current.Origin) < value) return false;
            return true;
         }

         public override String GetText()
         {
            return "Awarded for withstanding an acceleration of at least " + value + "g for " + GeeForceInspector.DURATION.ToString("0.0") + " or more seconds";
         }
      }

      class MachNumberAchievement : NumericAchievement
      {
         private static double MAX_ALTITUDE = 30000;
         public MachNumberAchievement(int value, int prestige)
            : base("M", "Mach " + Utils.Roman(value), value, prestige, false)
         {
         }

         public override bool Check(VesselState previous, VesselState current)
         {
            if (previous == null) return false;
            if (current == null) return false;
            if (current.Origin == null) return false;
            if (current.Origin.mainBody == null) return false;
            if (current.IsLanded) return false;
            if (current.IsEVA) return false;
            if (current.Situation == Vessel.Situations.PRELAUNCH) return false;
            if (current.Situation == Vessel.Situations.LANDED) return false;
            if (current.Situation == Vessel.Situations.SPLASHED) return false;
            if (current.Origin.altitude >= MAX_ALTITUDE) return false;
            double mach = current.Origin.MachNumberHorizontal();
            if (mach < value) return false;
            if (!current.Origin.mainBody.name.Equals("Kerbin") ) return false;
            return true;
         }

         public override String GetText()
         {
            return "Awarded for flying horizontally at mach " + value + " below "+MAX_ALTITUDE.ToString("0")+"m in Kerbin atmosphere";
         }
      }

      class DeepAtmosphereArchievement : CelestialBodyAchievement
      {

         public DeepAtmosphereArchievement(CelestialBody body, int prestige)
            : base("DA",body.GetName() + " Deep Atmosphere",body, prestige, false)
         {
         }

         public override bool Check(VesselState previous, VesselState current)
         {
            if (current == null) return false;
            if (current.atmDensity < 10.0) return false;
            return current.MainBody.Equals(body);
         }

         public override String GetText()
         {
            return "Awarded for entering the deeper atmosphere of " + body.GetName();
         }
      }

      class GrandTourAchievement : Achievement
      {
         public GrandTourAchievement(int prestige, bool first)
            : base("GT" + (first ? "1" : ""), "Grand Tour", prestige, first)
         {

         }

         // this class has no check methods, because the ribbon will be awarded directly

         public override String GetText()
         {
            return "Awarded for" + FirstKerbalText().Envelope() + "entering the sphere of influence of all celestial bodies in the kerbal system";
         }
      } 

      class JoolTourAchievement : Achievement
      {
         public JoolTourAchievement(int prestige, bool first)
            : base("JT" + (first ? "1" : ""), "Jool Tour", prestige, first)
         {

         }

         // this class has no check methods, because the ribbon will be awarded directly

         public override String GetText()
         {
            return "Awarded for" + FirstKerbalText().Envelope() + "entering the sphere of influence of all moons of Jool";
         }
      }

      // BROKEN
      class DeepSpaceAchievement : Achievement
      {
         CelestialBody outermostBodyInSystem;
         private readonly double minDistanceToSun;

         public DeepSpaceAchievement(int prestige, bool first)
            : base("DS" + (first ? "1" : ""), "Deep Space", prestige, first)
         {
            outermostBodyInSystem = GameUtils.GetOutermostPlanet();
            if (outermostBodyInSystem != null)
            {
               minDistanceToSun = outermostBodyInSystem.orbit.ApA + outermostBodyInSystem.sphereOfInfluence;
               Log.Detail("outermost planet for deep space ribbon is " + outermostBodyInSystem.name);
            }
            else
            {
               minDistanceToSun = 0;
               Log.Warning("no outermost planet foundfor deep space ribbon");
            }
         }

         public override bool Check(VesselState previous, VesselState current)
         {
            if (outermostBodyInSystem == null) return false;
            if(current==null) return false;
            if (current.MainBody == null) return true;
            if (!current.MainBody.IsSun()) return false;
            if (current.Origin == null) return false;
            if (GameUtils.GetDistanceToSun(current.Origin) < minDistanceToSun) return false; 
            // yeah! deep space!
            return true;
         }


         public override String GetText()
         {
            if (outermostBodyInSystem == null) return "no outermost planet found in system (ribbon not used)";
            return "Awarded for" + FirstKerbalText().Envelope() + "entering space beyond the orbit of " + outermostBodyInSystem.name;
         }
      }

      class ContractsAchievement : NumericAchievement
      {
         public ContractsAchievement(int value, int prestige)
            : base("N", "Multiple Contracts", value, prestige, false)
         {
         }

         public override bool Check(HallOfFameEntry entry) 
         {
            if (entry.ContractsCompleted < value) return false;
            return true;
         }


         public override String GetText()
         {
            return "Awarded for completing "+value +" or more contracts";
         }
      }

      class ResearchAchievement : NumericAchievement
      {
         public ResearchAchievement(int nr, int value, int prestige)
            : base("YR", "Research "+Utils.Roman(nr), value, prestige, false)
         {
         }

         public override bool Check(HallOfFameEntry entry)
         {
            if (entry.Research < value) return false;
            return true;
         }


         public override String GetText()
         {
            return "Awarded for researching " + value + " or more science points";
         }
      }


      abstract class ServiceAchievement : Achievement
      {
         private readonly String trait;

         public ServiceAchievement(String trait, String code, String name, int prestige)
            : base(code, name, prestige, false)
         {
            this.trait = trait;
         }

         public override bool Check(HallOfFameEntry entry)
         {
            ProtoCrewMember kerbal = entry.GetKerbal();
            if (kerbal == null) return false;
            if (!kerbal.experienceTrait.TypeName.Equals(trait)) return false;
            if (entry.MissionsFlown == 0) return false;
            return true;
         }
      }

      class ScientistServiceAchievement : ServiceAchievement
      {
         public ScientistServiceAchievement(int prestige)
            : base("Scientist","QS", "Scientific Service", prestige)
         {
         }

         public override String GetText()
         {
            return "Awarded for completing at least a single mission as a scientist";
         }
      }

      class EngineerServiceAchievement : ServiceAchievement
      {
         public EngineerServiceAchievement(int prestige)
            : base("Engineer", "QE", "Engineer Service", prestige)
         {
         }

         public override String GetText()
         {
            return "Awarded for completing at least a single mission as an engineer";
         }
      }

      class PilotServiceAchievement : ServiceAchievement
      {
         public PilotServiceAchievement(int prestige)
            : base("Pilot", "QO", "Operational Service", prestige)
         {
         }

         public override String GetText()
         {
            return "Awarded for completing at least a single mission as a pilot";
         }
      }

      class XMas2014Achievement : Achievement
      {
         private static readonly DateTime FirstTimeOfDuty = new DateTime(2014,12,24);
         private static readonly DateTime LastTimeOfDuty = new DateTime(2014,12,27);

         public XMas2014Achievement(int prestige)
            : base("X14", "X-mas 2014", prestige, false )
         {
         }

         public override bool Check(HallOfFameEntry entry)
         {
            DateTime now = DateTime.Now;
            if (now < FirstTimeOfDuty) return false;
            if (now >= LastTimeOfDuty) return false;
            ProtoCrewMember kerbal = entry.GetKerbal();
            if (kerbal == null) return false;
            return true;
         }

         public override bool Check(VesselState previous, VesselState current) 
         {
            DateTime now = DateTime.Now;
            if (now < FirstTimeOfDuty) return false;
            if (now >= LastTimeOfDuty) return false;
            return true; 
         }

         public override bool Check(EventReport report) 
         {
            DateTime now = DateTime.Now;
            if (now < FirstTimeOfDuty) return false;
            if (now >= LastTimeOfDuty) return false;
            return true; 
         }


         public override String GetText()
         {
            return "Awarded for any kind of duty on xmas 2014";
         }
      }

      class DateofYearAchievement : Achievement
      {
         private readonly String text;
         private readonly int firstDay;
         private readonly int firstMonth;
         private readonly int lastDay;
         private readonly int lastMonth;

         public DateofYearAchievement(int firstDay, int firstMonth, int lastDay, int lastMonth, String name, String text, int prestige)
            : base("Y" + ":" + firstDay + "." + firstMonth + "-" + lastDay + "." + lastMonth, name, prestige, false)
         {
            this.text = text;
            this.firstDay = firstDay;
            this.firstMonth = firstMonth;
            this.lastDay = lastDay;
            this.lastMonth = lastMonth;
         }

         private bool Check()
         {
            DateTime now = DateTime.Now;
            if (now.Month < firstMonth) return false;
            if (now.Month > lastMonth) return false;
            if (now.Day < firstDay) return false;
            if (now.Day > lastDay) return false;
            return true;
         }

         public override bool Check(HallOfFameEntry entry)
         {
            ProtoCrewMember kerbal = entry.GetKerbal();
            if (kerbal == null) return false;
            return Check();
         }

         public override bool Check(VesselState previous, VesselState current)
         {
            return Check();
         }

         public override bool Check(EventReport report)
         {
            return Check();
         }


         public override String GetText()
         {
            return text;
         }
      }

      class EvaInHomeWatersAchievement : Achievement
      {
         public EvaInHomeWatersAchievement(int prestige)
            : base("WE", "EVA in Kerbin waters", prestige, false)
         {
         }

         public override bool Check(VesselState previous, VesselState current)
         {
            // eva required 
            if (!current.IsEVA) return false;
            // Kerbin?
            if (current.MainBody != null || !current.MainBody.IsKerbin()) return false;
            // are we splashed?
            return current.Situation == Vessel.Situations.SPLASHED;
         }

         public override String GetText()
         {
            return "Awarded for any EVA in Kerbin waters";
         }
      }

      class CollisionAchievement : Achievement
      {
         public CollisionAchievement(int prestige)
            : base("C", "Collision", prestige, false)
         {
         }

         public override bool Check(EventReport report)
         {
            // is a collision event reported?
            if (!report.eventType.Equals(FlightEvents.COLLISION)) return false;
            // just checks if vessel is known
            if (report.origin == null) return false;
            if (report.origin.vessel == null) return false;
            // collsion in EVA wont count
            if (report.origin.vessel.isEVA) return false;
            // ok, this is a collision in a vessel
            return true;
         }

         public override String GetText()
         {
            return "Awarded for any collision while in a vessel";
         }
      }


      class WetEvaAchievement : Achievement
      {

         public WetEvaAchievement(int prestige, bool first = false)
            : base("EE"+(first?"1":""), "Wet EVA", prestige, first)
         {
         }

         public override bool Check(VesselState previous, VesselState current)
         {
            if (current == null) return false;
            // no EVA, no achievement
            if (!current.IsEVA) return false;
            if (current.Situation != Vessel.Situations.SPLASHED) return false;
            // no main celestial body? we have to be deep in space then (strange: water in deep space ;), but who knows?)
            if (current.MainBody == null) return true;
            // EVA on Kerbin is to easy
            if (current.MainBody.IsKerbin()) return false;
            return true;
         }

         public override String GetText()
         {
            return "Awarded for" + FirstKerbalText().Envelope() + "on an EVA in a wet environment outside of Kerbin";
         }
      }
   }
}