using System;

namespace _JoykadeGames.Runtime.SaveSystem
{
    public class SaveOperationContext : IDisposable
    {
        private readonly IWriterReader WriterReader;
        public SaveOperationContext(IWriterReader writerReader)
        {
            WriterReader = writerReader ?? throw new ArgumentNullException(nameof(writerReader));
            WriterReader.StartSaveOperation();
        }
        
        public void Dispose()
        {
            WriterReader.EndSaveOperation();
        }
    }
}