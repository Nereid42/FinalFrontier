using System;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;


namespace Nereid
{
   namespace FinalFrontier
   {
      public class VesselInspecteur : Inspecteur
      {
         private static readonly double MIN_INTERVAL = 0.20;

         public int wheelsGroundContact { get; private set; }

         private readonly List<Part> wheels = new List<Part>();

         public VesselInspecteur()
            : base(10, MIN_INTERVAL)
         {
            Reset();
         }


         public override void Reset()
         {
            this.wheels.Clear();
            this.wheelsGroundContact = 0;
         }

         protected void InspectVesselParts(Vessel vessel)
         {
            foreach (Part part in vessel.Parts)
            {
               if (part.packed) part.Unpack();
               foreach (PartModule m in part.Modules)
               {
                  // no clue how to detect a wheel right now...
               }
            }
         }

         protected override void ScanVessel(Vessel vessel)
         {
            this.wheelsGroundContact = 0;

            if (vessel == null)
            {
               wheels.Clear();
            }
            else
            {

               // inspect parts if present
               if (vessel != null && vessel.Parts != null)
               {
                  InspectVesselParts(vessel);
               }
            }
         }


         protected override void Inspect(Vessel vessel)
         {
            if(vessel==null)
            {
               this.wheelsGroundContact = 0;
            }
            else
            {
            }
         }
      }
   }
}
