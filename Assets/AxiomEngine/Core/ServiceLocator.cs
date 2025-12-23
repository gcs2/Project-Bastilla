// ============================================================================
// Axiom RPG Engine - Service Locator
// Copyright (c) Geoffrey Salmon 2025. All Rights Reserved.
// ============================================================================

using System;
using System.Collections.Generic;
using UnityEngine;

namespace RPGPlatform.Core
{
    /// <summary>
    /// Simple service locator pattern for registering and retrieving game services
    /// Provides global access to core systems like morality, combat resolver, etc.
    /// </summary>
    public static class ServiceLocator
    {
        private static Dictionary<Type, object> _services = new Dictionary<Type, object>();
        
        /// <summary>
        /// Register a service instance
        /// </summary>
        public static void Register<T>(T service) where T : class
        {
            var type = typeof(T);
            
            if (_services.ContainsKey(type))
            {
                Debug.LogWarning($"[ServiceLocator] Service {type.Name} is already registered. Overwriting.");
            }
            
            _services[type] = service;
            Debug.Log($"[ServiceLocator] Registered service: {type.Name}");
        }
        
        /// <summary>
        /// Get a registered service
        /// </summary>
        public static T Get<T>() where T : class
        {
            var type = typeof(T);
            
            if (_services.TryGetValue(type, out var service))
            {
                return service as T;
            }
            
            Debug.LogWarning($"[ServiceLocator] Service {type.Name} not found");
            return null;
        }
        
        /// <summary>
        /// Try to get a service without logging warnings
        /// </summary>
        public static bool TryGet<T>(out T service) where T : class
        {
            var type = typeof(T);
            
            if (_services.TryGetValue(type, out var obj))
            {
                service = obj as T;
                return service != null;
            }
            
            service = null;
            return false;
        }
        
        /// <summary>
        /// Check if a service is registered
        /// </summary>
        public static bool IsRegistered<T>() where T : class
        {
            return _services.ContainsKey(typeof(T));
        }
        
        /// <summary>
        /// Unregister a service
        /// </summary>
        public static void Unregister<T>() where T : class
        {
            var type = typeof(T);
            
            if (_services.Remove(type))
            {
                Debug.Log($"[ServiceLocator] Unregistered service: {type.Name}");
            }
        }
        
        /// <summary>
        /// Clear all registered services
        /// </summary>
        public static void Clear()
        {
            _services.Clear();
            Debug.Log("[ServiceLocator] Cleared all services");
        }
        
        /// <summary>
        /// Get count of registered services
        /// </summary>
        public static int Count => _services.Count;
    }
}
