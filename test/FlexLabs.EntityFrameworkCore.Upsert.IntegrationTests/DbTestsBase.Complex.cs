using FlexLabs.EntityFrameworkCore.Upsert.IntegrationTests.Base;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace FlexLabs.EntityFrameworkCore.Upsert.Tests.EF
{
    public partial class DbTestsBase
    {
        // base value that is inserted at db startup
        private readonly ComplexOwner _dbComplexOwner = new ComplexOwner
        {
            ID = 1,
            Complex = new ComplexValue
            {
                Value1 = 1,
                Value2 = 2,
                Sub = new SubComplexValue
                {
                    Innermost = 3
                }
            }
        };
        
        [SkippableFact]
        public virtual void Upsert_ComplexProperty_NoUpdate()
        {
            Skip.If(_fixture.DbDriver is DbDriver.InMemory, "db doesn't support complex properties");

            ResetDb();
            using var dbContext = new TestDbContext(_fixture.DataContextOptions);

            // attempt to upsert to existing ID with NoUpdate, nothing should happen
            var dataToUpsert = _dbComplexOwner with
            {
                Complex = new ComplexValue
                {
                    Value1 = 9
                }
            };
            dbContext.ComplexOwners
                .Upsert(dataToUpsert)
                .On(p => p.ID)
                .NoUpdate()
                .Run();

            Assert.Single(dbContext.ComplexOwners, _dbComplexOwner);
        }
        
        [SkippableFact]
        public virtual void Upsert_ComplexProperty_Update()
        {
            Skip.If(_fixture.DbDriver is DbDriver.InMemory, "db doesn't support complex properties");

            ResetDb();
            using var dbContext = new TestDbContext(_fixture.DataContextOptions);
            
            var dataToUpsert = _dbComplexOwner with
            {
                Complex = new ComplexValue
                {
                    Value1 = 9
                }
            };
            
            dbContext.ComplexOwners
                .Upsert(dataToUpsert)
                .On(p => p.ID)
                .Run();
            
            Assert.Single(dbContext.ComplexOwners, dataToUpsert);
        }
        
        // todo: MatchColumnsHaveToBePropertiesOfTheTEntityClass
        /*[SkippableFact]
        public virtual void Upsert_ComplexProperty_OnComplex()
        {
            Skip.If(_fixture.DbDriver is DbDriver.InMemory, "db doesn't support complex properties");

            ResetDb();
            using var dbContext = new TestDbContext(_fixture.DataContextOptions);
            
            var dataToUpsert = _dbComplexOwner with
            {
                Complex = new ComplexValue
                {
                    Value1 = 9
                }
            };
            
            dbContext.ComplexOwners
                .Upsert(dataToUpsert)
                .On(p => p.Complex.Value2) // !
                .Run();
            
            Assert.Single(dbContext.ComplexOwners, dataToUpsert);
        }*/
        
        [SkippableFact]
        public virtual void Upsert_ComplexProperty_Increment()
        {
            Skip.If(_fixture.DbDriver is DbDriver.InMemory, "db doesn't support complex properties");

            ResetDb();
            using var dbContext = new TestDbContext(_fixture.DataContextOptions);
            
            dbContext.ComplexOwners
                .Upsert(_dbComplexOwner)
                .On(p => p.ID)
                .WhenMatched(e => new ComplexOwner
                {
                    Complex = new ComplexValue
                    {
                        Value1 = e.Complex.Value1 + 1
                    }
                })
                .Run();
            
            var expected = _dbComplexOwner with
            {
                Complex = _dbComplexOwner.Complex with
                {
                    Value1 = _dbComplexOwner.Complex.Value1 + 1
                }
            };
            Assert.Single(dbContext.ComplexOwners, expected);
        }
        
        [SkippableFact]
        public virtual void Upsert_ComplexProperty_Replace()
        {
            Skip.If(_fixture.DbDriver is DbDriver.InMemory, "db doesn't support complex properties");

            ResetDb();
            using var dbContext = new TestDbContext(_fixture.DataContextOptions);
            
            var dataToUpsert = _dbComplexOwner with
            {
                Complex = new ComplexValue
                {
                    Value1 = 9,
                    Value2 = 500,
                    Sub = new SubComplexValue
                    {
                        Innermost = 7
                    }
                }
            };
            
            dbContext.ComplexOwners
                .Upsert(dataToUpsert)
                .On(p => p.ID)
                .WhenMatched((existing, requested) => new ComplexOwner
                {
                    // writes to all child columns
                    Complex = requested.Complex
                })
                .Run();
            
            Assert.Single(dbContext.ComplexOwners, dataToUpsert);
        }
    }
}