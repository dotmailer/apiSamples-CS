namespace dotMailer.PrintAccountInfo
{
    internal class ApiAccount
    {
        public int ID
        {
            get;
            set;
        }

        public ApiAccountProperty[] Properties
        {
            get;
            set;
        }
    }
}