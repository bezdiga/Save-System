
using nn;
using nn.fs;
using UnityEngine;

namespace _JoykadeGames.Runtime.SaveSystem.Nitendo
{
    public static class SwitchFsErrorHandler
    {
        public static bool Check(Result result, out FileSystemErrorType errorType)
        {
            if (result.IsSuccess())
            {
                errorType = FileSystemErrorType.None;
                return true;
            }
            
            string baseErrorMessage = $"SaveSystem Error: Failed.";
            
            if (FileSystem.ResultPathNotFound.Includes(result))
            {
                Debug.LogError($"{baseErrorMessage} Reason: Path not found. Details: {result}");
                errorType = FileSystemErrorType.PathNotFound;
            }
            else if (FileSystem.ResultUsableSpaceNotEnough.Includes(result))
            {
                Debug.LogError($"{baseErrorMessage} Reason: Not enough usable space. Details: {result}");
                errorType = FileSystemErrorType.UsableSpaceNotEnough;
            }
            else if (FileSystem.ResultTargetLocked.Includes(result))
            {
                Debug.LogError($"{baseErrorMessage} Reason: Target file is locked. Details: {result}");
                errorType = FileSystemErrorType.TargetLocked;
            }
            else
            {
                // Pentru orice altă eroare
                Debug.LogError($"{baseErrorMessage} Reason: An unexpected error occurred. Details: {result}");
                errorType = FileSystemErrorType.Unknown;
            }
            return false;
        }
    }
}