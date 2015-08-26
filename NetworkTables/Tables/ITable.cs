﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetworkTables.Tables
{
    public interface ITable
    {
        string Path { get; }

        /// <summary>
        /// Determines whether the given key is in this table.
        /// </summary>
        /// <param name="key">The key to search for</param>
        /// <returns>If the table has a value assignend to the given key</returns>
        bool ContainsKey(string key);

        /// <summary>
        /// Determines whether there exists a non-empty subtable for this key in this table.
        /// </summary>
        /// <param name="key">The key to search for</param>
        /// <returns>If there is a subtable with the key which contains at least one key/subtable of its own</returns>
        bool ContainsSubTable(string key);

        /// <summary>
        /// Gets the subtable in this table for the given name.
        /// </summary>
        /// <param name="key">The name of the table relative to this one.</param>
        /// <returns>A sub table relative to this one</returns>
        ITable GetSubTable(string key);

        /// <summary>
        /// Makes a key's value persistent through program restarts.
        /// </summary>
        /// <param name="key">The key to make persistent</param>
        void Persist(string key);

        /// <summary>
        /// Gets the value associated with a key as an object
        /// </summary>
        /// <param name="key">The key of the value to look up</param>
        /// <returns>The value associated with the given key, or null if the key does not exist.</returns>
        object GetValue(string key);

        /// <summary>
        /// Put a value in the table.
        /// </summary>
        /// <param name="key">The key to be assigned to</param>
        /// <param name="value">The value that will be assigned</param>
        void PutValue(string key, object value);
        
        /// <summary>
        /// Put a number in the table.
        /// </summary>
        /// <param name="key">The key to be assigned to</param>
        /// <param name="value">The value that will be assigned</param>
        void PutNumber(string key, double value);

        /// <summary>
        /// Gets the number associated with the given name.
        /// </summary>
        /// <param name="key">The key to look up</param>
        /// <param name="defaultValue">The value to be returned if no value is found</param>
        /// <returns>The value associated with the given key, or the given default value if there is no value associated with the key</returns>
        double GetNumber(string key, double defaultValue);

        /// <summary>
        /// Put a string in the table.
        /// </summary>
        /// <param name="key">The key to be assigned to</param>
        /// <param name="value">The value that will be assigned</param>
        void PutString(string key, string value);

        /// <summary>
        /// Gets the string associated with the given name.
        /// </summary>
        /// <param name="key">The key to look up</param>
        /// <param name="defaultValue">The value to be returned if no value is found</param>
        /// <returns>The value associated with the given key, or the given default value if there is no value associated with the key</returns>
        string GetString(string key, string defaultValue);

        /// <summary>
        /// Put a boolean in the table.
        /// </summary>
        /// <param name="key">The key to be assigned to</param>
        /// <param name="value">The value that will be assigned</param>
        void PutBoolean(string key, bool value);

        /// <summary>
        /// Gets the boolean associated with the given name.
        /// </summary>
        /// <param name="key">The key to look up</param>
        /// <param name="defaultValue">The value to be returned if no value is found</param>
        /// <returns>The value associated with the given key, or the given default value if there is no value associated with the key</returns>
        bool GetBoolean(string key, bool defaultValue);

        /// <summary>
        /// Add a listener to changes to the table.
        /// </summary>
        /// <param name="listener">The listener to add</param>
        /// <param name="immediateNotify">If true then this listener will be notified of all current entries (marked as new)</param>
        void AddTableListener(ITableListener listener, bool immediateNotify = false);

        /// <summary>
        /// Add a listener for changes to a specific key in the table.
        /// </summary>
        /// <param name="key">The key to listen for</param>
        /// <param name="listener">The listener to add</param>
        /// <param name="immediateNotify">If true then this listener will be notified of all current entries (marked as new)</param>
        void AddTableListener(string key, ITableListener listener, bool immediateNotify);

        /// <summary>
        /// Remove a listener from receiving table events.
        /// </summary>
        /// <param name="listener">The listener to be removed.</param>
        void RemoveTableListener(ITableListener listener);
    }
}
