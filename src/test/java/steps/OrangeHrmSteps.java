package steps;

import io.cucumber.java.en.*;
import models.EmployeeData;
import pages.DashboardPage;
import pages.DirectoryPage;
import pages.LoginPage;
import pages.PimPage;

public class OrangeHrmSteps {

    LoginPage loginPage;
    DashboardPage dashboardPage;
    PimPage pimPage;
    DirectoryPage directoryPage;

    private final String ADMIN_USER = "Admin";
    private final String ADMIN_PASSWORD = "admin123";
    private final String PHOTO_PATH = "src/test/resources/testdata/profile.jpg";

    @Given("el usuario ingresa a OrangeHRM con credenciales de administrador")
    public void loginAsAdmin() {
        loginPage.openLoginPage();

        if (!loginPage.isLoginPageLoaded()) {
            throw new AssertionError("La página de login no cargó correctamente");
        }

        loginPage.login(ADMIN_USER, ADMIN_PASSWORD);

        try {
            Thread.sleep(6000);
        } catch (InterruptedException e) {
            e.printStackTrace();
        }

        if (!dashboardPage.isDashboardLoaded()) {
            throw new AssertionError("No se cargó el dashboard después del login");
        }
    }

    @When("navega al modulo PIM")
    public void goToPim() {
        dashboardPage.goToPim();

        if (!pimPage.isPimLoaded()) {
            throw new AssertionError("El módulo PIM no cargó correctamente");
        }
    }

    @When("agrega un nuevo empleado con nombre {string} y apellido {string}")
    public void createEmployee(String firstName, String lastName) {
        EmployeeData.firstName = firstName;
        EmployeeData.lastName = lastName;
        EmployeeData.fullName = firstName + " " + lastName;

        pimPage.clickAddEmployee();
        pimPage.createEmployee(firstName, lastName, PHOTO_PATH);

        if (!pimPage.employeeProfileLoaded(firstName, lastName)) {
            throw new AssertionError("No se cargó el perfil del empleado: " + EmployeeData.fullName);
        }
    }

    @When("sube una foto de perfil")
    public void uploadPhoto() {
        // La foto ya se sube en createEmployee() para mantener el flujo estable.
    }

    @When("navega al modulo Directory")
    public void goToDirectory() {
        dashboardPage.goToDirectory();
    }

    @When("busca el empleado creado")
    public void searchEmployee() {
        directoryPage.searchEmployee(EmployeeData.firstName);
    }

    @Then("valida que la informacion basica del empleado sea correcta")
    public void validateEmployee() {
        if (!directoryPage.employeeAppearsInResults(EmployeeData.firstName, EmployeeData.lastName)) {
            throw new AssertionError("El empleado no aparece en Directory: " + EmployeeData.fullName);
        }
    }
}