namespace GCL
{
    public static class Singleton<T> where T : class, new()
    {
        private static T _instance;
        public static T Instance
        {
            get
            {
                _instance ??= new T();
                return _instance;
            }
        }
    }
}