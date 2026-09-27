using OrangeHrm.Tests.Helpers;
using Xunit;

namespace OrangeHrm.Tests.Tests;

public class DatabaseHelperTests
{
    [Fact]
    public void DatabaseHelper_AdoNet_And_Dapper_Workflow_ShouldSucceed()
    {
        using var dbHelper = new DatabaseHelper();

        // 1. Alta (Insert)
        dbHelper.RecordEmployeeCreated("Carlos", "Tester", "QA");
        int countAfterInsert = dbHelper.GetEmployeeCount("Carlos", "Tester");
        Assert.Equal(1, countAfterInsert);

        // 2. Modificación (Update)
        dbHelper.RecordEmployeeUpdated("Carlos", "SeniorQA");
        string? updatedMiddleName = dbHelper.GetEmployeeMiddleName("Carlos");
        Assert.Equal("SeniorQA", updatedMiddleName);

        // 3. Verificación con ADO.NET puro
        bool existsInAdoNet = dbHelper.ExistsByAdoNet("Carlos", "Tester");
        Assert.True(existsInAdoNet);

        // 4. Baja (Delete)
        dbHelper.RecordEmployeeDeleted("Carlos");
        int countAfterDelete = dbHelper.GetEmployeeCount("Carlos", "Tester");
        Assert.Equal(0, countAfterDelete);
    }
}
