using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nereid
{
   namespace FinalFrontier
   {
      public abstract class Permutation<T>
      {
         public abstract T Get(T[] array, int index);
      }

      public class Identity<T> : Permutation<T>
      {
        /*public Identity()
         {

         }*/

         public override T Get(T[] array, int index)
         {
            return array[index];
         }
      }

      public class ArrayPermutation<T> : Permutation<T>
      {
         private readonly int[] permutation;

         public ArrayPermutation(int[] permutation)
         {
            this.permutation = permutation;

            // test permutation
            bool[] found = new bool[permutation.Length];
            for(int i=0; i<permutation.Length; i++)
            {
               int index = permutation[i];
               if(found[index])
               {
                  throw new Exception("multiple permutation index: "+index);
               }
               found[index] = true;
            }
         }

         public override T Get(T[] array, int index)
         {
            if (permutation.Length <= index)
            {
               return array[index];
            }
            return array[permutation[index]];
         }
      }
   }
}
