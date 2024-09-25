namespace Obvious.Soap
{
    public abstract class ScriptableSaveBase : ScriptableBase
    {
        public abstract void CheckAndSave();
        public abstract void Save();
        public abstract void Load();
        
        public enum ELoadMode
        {
            Automatic,
            Manual
        }
        public enum ESaveMode
        {
            Manual,
            Interval
        }
    }
}