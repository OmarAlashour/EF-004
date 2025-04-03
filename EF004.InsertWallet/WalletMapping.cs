using NHibernate;
using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;
using System.Runtime.CompilerServices;

namespace EF004.InsertWallet;

public class WalletMapping: ClassMapping<Wallet>
{
    public WalletMapping()
    {
        Table("Wallets");
        
        Id(x => x.Id, m => {
            m.Generator(Generators.Identity);
            m.Column("Id");
            m.UnsavedValue(0);
        });

        Property(x => x.Holder, c =>
        {
            c.Length(50);
            c.Type(NHibernateUtil.AnsiString);
            c.NotNullable(true);
        });

        Property(x => x.Balance, c =>
        { 
            c.Type(NHibernateUtil.Decimal);
            c.NotNullable(true);
        });
    } 
}
