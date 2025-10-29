using System;
using System.Threading.Tasks;
using UnityEditor;

/// <summary>
/// Handles synchronization of operations back to Unity's main thread
/// </summary>
public class MainThreadSynchronizer
{
    /// <summary>
    /// Executes an action on Unity's main thread
    /// </summary>
    public async Task ExecuteOnMainThread(Action action)
    {
        if (action == null)
            throw new ArgumentNullException(nameof(action));
        
        var completionSource = new TaskCompletionSource<bool>();
        
        // Use Unity's EditorApplication.update to execute on main thread
        EditorApplication.CallbackFunction callback = null;
        callback = () =>
        {
            try
            {
                action();
                completionSource.SetResult(true);
            }
            catch (Exception ex)
            {
                completionSource.SetException(ex);
            }
            finally
            {
                EditorApplication.update -= callback;
            }
        };
        
        EditorApplication.update += callback;
        
        await completionSource.Task;
    }
    
    /// <summary>
    /// Executes a function on Unity's main thread and returns the result
    /// </summary>
    public async Task<T> ExecuteOnMainThread<T>(Func<T> function)
    {
        if (function == null)
            throw new ArgumentNullException(nameof(function));
        
        var completionSource = new TaskCompletionSource<T>();
        
        EditorApplication.CallbackFunction callback = null;
        callback = () =>
        {
            try
            {
                T result = function();
                completionSource.SetResult(result);
            }
            catch (Exception ex)
            {
                completionSource.SetException(ex);
            }
            finally
            {
                EditorApplication.update -= callback;
            }
        };
        
        EditorApplication.update += callback;
        
        return await completionSource.Task;
    }
    
    /// <summary>
    /// Schedules an action to be executed on the next main thread update
    /// </summary>
    public void ScheduleOnMainThread(Action action)
    {
        if (action == null)
            throw new ArgumentNullException(nameof(action));
        
        EditorApplication.CallbackFunction callback = null;
        callback = () =>
        {
            try
            {
                action();
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError($"BentleyAuthManager_Editor: Error executing scheduled action: {ex.Message}");
            }
            finally
            {
                EditorApplication.update -= callback;
            }
        };
        
        EditorApplication.update += callback;
    }
    
    /// <summary>
    /// Checks if the current thread is Unity's main thread
    /// </summary>
    public bool IsMainThread()
    {
        return System.Threading.Thread.CurrentThread.ManagedThreadId == 1;
    }
    
    /// <summary>
    /// Executes an action immediately if on main thread, otherwise schedules it
    /// </summary>
    public void ExecuteOnMainThreadImmediate(Action action)
    {
        if (action == null)
            throw new ArgumentNullException(nameof(action));
        
        if (IsMainThread())
        {
            try
            {
                action();
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError($"BentleyAuthManager_Editor: Error executing immediate action: {ex.Message}");
            }
        }
        else
        {
            ScheduleOnMainThread(action);
        }
    }
}
