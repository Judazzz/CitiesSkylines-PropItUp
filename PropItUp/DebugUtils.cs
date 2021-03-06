﻿using ColossalFramework.Plugins;
using System;
using System.ComponentModel;
using UnityEngine;

namespace PropItUp
{
    class DebugUtils
    {
        public const string modPrefix = "[Prop it Up! " + Mod.version + "] ";

        public static void Message(string message)
        {
            Log(message);
            DebugOutputPanel.AddMessage(PluginManager.MessageType.Message, modPrefix + message);
        }

        public static void Warning(string message)
        {
            Debug.LogWarning(modPrefix + message);
            DebugOutputPanel.AddMessage(PluginManager.MessageType.Warning, modPrefix + message);
        }

        public static void Log(string message)
        {
            if (message == m_lastLog)
            {
                m_duplicates++;
            }
            else if (m_duplicates > 0)
            {
                Debug.Log(modPrefix + "(x" + (m_duplicates + 1) + ")");
                Debug.Log(modPrefix + message);
                m_duplicates = 0;
            }
            else
            {
                Debug.Log(modPrefix + message);
            }
            m_lastLog = message;
        }

        public static void LogException(Exception e)
        {
            var message = $"{modPrefix}Unexpected {e.GetType().Name}: {e.Message}\n{e.StackTrace}\n\nInnerException:\n{e.InnerException.Message}";
            Log(message);
        }

        private static string m_lastLog;
        private static int m_duplicates = 0;

        public void dumpObject(object myObject)
        {
            string myObjectDetails = "";
            foreach (PropertyDescriptor descriptor in TypeDescriptor.GetProperties(myObject))
            {
                string name = descriptor.Name;
                object value = descriptor.GetValue(myObject);
                myObjectDetails += name + ": " + value + "\n";
            }
            DebugOutputPanel.AddMessage(PluginManager.MessageType.Message, myObjectDetails);
        }
    }
}