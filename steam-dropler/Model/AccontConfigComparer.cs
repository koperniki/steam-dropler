using System.Collections.Generic;

namespace steam_dropler.Model
{
    /// <summary>
    /// Сравнение аккаунтов
    /// </summary>
    internal class AccontConfigComparer : IEqualityComparer<AccountConfig>
    {
        public bool Equals(AccountConfig x, AccountConfig y)
        {
            if (x.Name == y.Name)
                return true;

            return false;
        }

        public int GetHashCode(AccountConfig obj)
        {
            return obj.Name.GetHashCode();
        }
    }
}