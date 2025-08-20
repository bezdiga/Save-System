using System;
using nn.fs;
using UnityEngine;

namespace _JoykadeGames.Runtime.SaveSystem
{
    public class WriteContext : IDisposable
    {
        private readonly string mountName;
        public nn.fs.FileHandle handle;
        public WriteContext(string mountName)
        {
            this.mountName = mountName;
            StartWrite();
        }
        public void Dispose()
        {
            EndWrite();
        }

        void StartWrite()
        {
            UnityEngine.Switch.Notification.EnterExitRequestHandlingSection();
            handle = new FileHandle();
            Debug.Log("Entering exit request handling section...");
        }

        void EndWrite()
        {
            nn.fs.File.Close(handle);
            var result = nn.fs.FileSystem.Commit(mountName);
            result.abortUnlessSuccess();
            UnityEngine.Switch.Notification.LeaveExitRequestHandlingSection();
            Debug.Log("Ending exit request handling section...");
        }
    }
}