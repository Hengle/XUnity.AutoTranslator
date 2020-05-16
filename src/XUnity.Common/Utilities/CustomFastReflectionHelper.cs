﻿using Mono.Cecil;
using Mono.Cecil.Cil;
using MonoMod.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using XUnity.Common.Constants;

namespace XUnity.Common.Utilities
{
   /// <summary>
   /// WARNING: Pubternal API (internal). Do not use. May change during any update.
   /// </summary>
   public static class CustomFastReflectionHelper
   {
      private static readonly Dictionary<FastReflectionDelegateKey, FastReflectionDelegate> _MethodCache = new Dictionary<FastReflectionDelegateKey, FastReflectionDelegate>();

      /// <summary>
      /// WARNING: Pubternal API (internal). Do not use. May change during any update.
      /// </summary>
      /// <param name="method"></param>
      /// <param name="directBoxValueAccess"></param>
      /// <param name="forceNonVirtCall"></param>
      /// <returns></returns>
      public static FastReflectionDelegate CreateFastDelegate( this MethodBase method, bool directBoxValueAccess = true, bool forceNonVirtCall = false )
      {
         var key = new FastReflectionDelegateKey( method, directBoxValueAccess, forceNonVirtCall );

         if( _MethodCache.TryGetValue( key, out FastReflectionDelegate dmd ) )
            return dmd;

         if( ClrTypes.DynamicMethodDefinition != null )
         {
            dmd = GetFastDelegateForCecil( method, directBoxValueAccess, forceNonVirtCall );
         }
         else
         {
            dmd = ReflectionEmitFastReflectionHelper.CreateFastDelegate( method, directBoxValueAccess, forceNonVirtCall );
         }

         _MethodCache.Add( key, dmd );
         return dmd;
      }

      /// <summary>
      /// WARNING: Pubternal API (internal). Do not use. May change during any update.
      /// </summary>
      /// <typeparam name="T"></typeparam>
      /// <typeparam name="F"></typeparam>
      /// <param name="fieldInfo"></param>
      /// <returns></returns>
      public static Func<T, F> CreateFastFieldGetter<T, F>( FieldInfo fieldInfo )
      {
         if( ClrTypes.DynamicMethodDefinition != null )
         {
            return CreateFastFieldGetterForCecil<T, F>( fieldInfo );
         }
         else
         {
            return ReflectionEmitFastReflectionHelper.CreateFastFieldGetter<T, F>( fieldInfo );
         }
      }

      /// <summary>
      /// WARNING: Pubternal API (internal). Do not use. May change during any update.
      /// </summary>
      /// <typeparam name="T"></typeparam>
      /// <typeparam name="F"></typeparam>
      /// <param name="fieldInfo"></param>
      /// <returns></returns>
      public static Action<T, F> CreateFastFieldSetter<T, F>( FieldInfo fieldInfo )
      {
         if( ClrTypes.DynamicMethodDefinition != null )
         {
            return CreateFastFieldSetterForCecil<T, F>( fieldInfo );
         }
         else
         {
            return ReflectionEmitFastReflectionHelper.CreateFastFieldSetter<T, F>( fieldInfo );
         }
      }

      // wrapped method calls to ensure the class is not initialized if unavailable
      private static FastReflectionDelegate GetFastDelegateForCecil( MethodBase method, bool directBoxValueAccess, bool forceNonVirtCall )
      {
         return CecilFastReflectionHelper.CreateFastDelegate( method, directBoxValueAccess, forceNonVirtCall ); ;
      }
      private static Func<T, F> CreateFastFieldGetterForCecil<T, F>( FieldInfo fieldInfo )
      {
         return CecilFastReflectionHelper.CreateFastFieldGetter<T, F>( fieldInfo );
      }
      private static Action<T, F> CreateFastFieldSetterForCecil<T, F>( FieldInfo fieldInfo )
      {
         return CecilFastReflectionHelper.CreateFastFieldSetter<T, F>( fieldInfo );
      }

      private struct FastReflectionDelegateKey
      {
         public FastReflectionDelegateKey( MethodBase method, bool directBoxValueAccess, bool forceNonVirtCall )
         {
            Method = method;
            DirectBoxValueAccess = directBoxValueAccess;
            ForceNonVirtCall = forceNonVirtCall;
         }

         public MethodBase Method { get; }
         public bool DirectBoxValueAccess { get; }
         public bool ForceNonVirtCall { get; }

         public override bool Equals( object obj )
         {
            return obj is FastReflectionDelegateKey key &&
                    EqualityComparer<MethodBase>.Default.Equals( Method, key.Method ) &&
                     DirectBoxValueAccess == key.DirectBoxValueAccess &&
                     ForceNonVirtCall == key.ForceNonVirtCall;
         }

         public override int GetHashCode()
         {
            var hashCode = 1017116076;
            hashCode = hashCode * -1521134295 + EqualityComparer<MethodBase>.Default.GetHashCode( Method );
            hashCode = hashCode * -1521134295 + DirectBoxValueAccess.GetHashCode();
            hashCode = hashCode * -1521134295 + ForceNonVirtCall.GetHashCode();
            return hashCode;
         }
      }
   }
}
