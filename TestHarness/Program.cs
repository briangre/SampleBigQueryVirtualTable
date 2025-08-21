using System;
using Microsoft.Xrm.Sdk;
using TestHarness.MockServices;

namespace TestHarness
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("BigQuery Virtual Table Test Harness");
            Console.WriteLine("=====================================");

            try
            {
                // Test RetrieveMultiple
                TestRetrieveMultiple();

                // Test Retrieve
                //TestRetrieve();

                // Test Create
                //TestCreate();

                // Test Update
                //TestUpdate();

                // Test Delete
                //TestDelete();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
            }

            Console.WriteLine("\nPress any key to exit...");
            Console.ReadKey();
        }

        static void TestRetrieveMultiple()
        {
            Console.WriteLine("\n--- Testing RetrieveMultiple ---");

            var context = new MockPluginExecutionContext
            {
                PrimaryEntityName = "Schedule"
            };

            var serviceProvider = new MockServiceProvider(
                context,
                new MockOrganizationServiceFactory(),
                new MockTracingService()
            );

            var plugin = new RetrieveMultiplePlugin();
            plugin.Execute(serviceProvider);

            if (context.OutputParameters.Contains("BusinessEntityCollection"))
            {
                var collection = (EntityCollection)context.OutputParameters["BusinessEntityCollection"];
                Console.WriteLine($"Retrieved {collection.Entities.Count} records");
                Console.WriteLine("First record details:");
                if (collection.Entities.Count > 0)
                {
                    var firstEntity = collection.Entities[0];
                    Console.WriteLine($"Logical Name: {firstEntity.LogicalName}");
                    foreach (var attribute in firstEntity.Attributes)
                    {
                        Console.WriteLine($"  {attribute.Key}: {attribute.Value}");
                    }
                }
                else
                {
                    Console.WriteLine("No records found.");
                }
            }
        }

        static void TestRetrieve()
        {
            Console.WriteLine("\n--- Testing Retrieve ---");

            var context = new MockPluginExecutionContext
            {
                PrimaryEntityName = "schedules"
            };
            context.InputParameters["Id"] = Guid.NewGuid();

            var serviceProvider = new MockServiceProvider(
                context,
                new MockOrganizationServiceFactory(),
                new MockTracingService()
            );

            var plugin = new RetrievePlugin();
            plugin.Execute(serviceProvider);

            if (context.OutputParameters.Contains("BusinessEntity"))
            {
                var entity = (Entity)context.OutputParameters["BusinessEntity"];
                Console.WriteLine($"Retrieved entity: {entity.LogicalName}");
            }
        }

        static void TestCreate()
        {
            Console.WriteLine("\n--- Testing Create ---");

            var inputEntity = new Entity("schedules");
            inputEntity["name"] = "Test Schedule";
            inputEntity["date"] = DateTime.Today;

            var context = new MockPluginExecutionContext
            {
                PrimaryEntityName = "schedules"
            };
            context.InputParameters["Target"] = inputEntity;

            var serviceProvider = new MockServiceProvider(
                context,
                new MockOrganizationServiceFactory(),
                new MockTracingService()
            );

            var plugin = new CreatePlugin();
            plugin.Execute(serviceProvider);

            Console.WriteLine("Create operation completed");
        }

        static void TestUpdate()
        {
            Console.WriteLine("\n--- Testing Update ---");

            var inputEntity = new Entity("schedules")
            {
                Id = Guid.NewGuid()
            };
            inputEntity["name"] = "Updated Schedule";

            var context = new MockPluginExecutionContext
            {
                PrimaryEntityName = "schedules"
            };
            context.InputParameters["Target"] = inputEntity;

            var serviceProvider = new MockServiceProvider(
                context,
                new MockOrganizationServiceFactory(),
                new MockTracingService()
            );

            var plugin = new UpdatePlugin();
            plugin.Execute(serviceProvider);

            Console.WriteLine("Update operation completed");
        }

        static void TestDelete()
        {
            Console.WriteLine("\n--- Testing Delete ---");

            var context = new MockPluginExecutionContext
            {
                PrimaryEntityName = "schedules"
            };
            context.InputParameters["Id"] = Guid.NewGuid();

            var serviceProvider = new MockServiceProvider(
                context,
                new MockOrganizationServiceFactory(),
                new MockTracingService()
            );

            var plugin = new DeletePlugin();
            plugin.Execute(serviceProvider);

            Console.WriteLine("Delete operation completed");
        }
    }
}
