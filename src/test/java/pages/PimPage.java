package pages;

import net.serenitybdd.core.pages.PageObject;
import net.serenitybdd.core.pages.WebElementFacade;
import net.thucydides.core.webdriver.WebDriverFacade;
import org.openqa.selenium.By;
import org.openqa.selenium.JavascriptExecutor;
import org.openqa.selenium.WebDriver;
import org.openqa.selenium.remote.LocalFileDetector;
import org.openqa.selenium.remote.RemoteWebDriver;

import java.io.File;

public class PimPage extends PageObject {

    private final By pimHeader = By.xpath("//h6[contains(normalize-space(),'PIM')]");
    private final By addEmployeeOption = By.xpath("//a[contains(@href,'addEmployee') or normalize-space()='Add Employee']");
    private final By firstNameInput = By.name("firstName");
    private final By lastNameInput = By.name("lastName");
    private final By photoInput = By.cssSelector("input[type='file']");
    private final By saveButton = By.cssSelector("button[type='submit']");

    public void clickAddEmployee() {
        waitABit(6000);

        if (!isPimLoaded()) {
            throw new AssertionError("El módulo PIM no cargó correctamente");
        }

        WebElementFacade addEmployee = find(addEmployeeOption);
        addEmployee.waitUntilPresent();
        waitABit(2000);

        try {
            addEmployee.waitUntilClickable().click();
        } catch (Exception e) {
            ((JavascriptExecutor) getDriver()).executeScript("arguments[0].click();", addEmployee);
        }

        waitForAddEmployeeForm();
    }

    public void createEmployee(String firstName, String lastName, String photoPath) {
        waitForAddEmployeeForm();

        find(firstNameInput).waitUntilVisible().type(firstName);
        find(lastNameInput).waitUntilVisible().type(lastName);

        File file = new File(photoPath);
        String absolutePath = file.getAbsolutePath();

        if (!file.exists()) {
            throw new RuntimeException("El archivo no existe: " + absolutePath);
        }

        RemoteWebDriver remoteDriver = getUnderlyingRemoteDriver();
        remoteDriver.setFileDetector(new LocalFileDetector());

        WebElementFacade upload = find(photoInput);
        upload.waitUntilPresent();
        upload.sendKeys(absolutePath);

        waitABit(3000);

        find(saveButton).waitUntilClickable().click();
        waitABit(4000);
    }

    public boolean isPimLoaded() {
        waitABit(5000);
        return findAll(pimHeader).size() > 0 || findAll(addEmployeeOption).size() > 0;
    }

    public boolean employeeProfileLoaded(String firstName, String lastName) {
        return find(By.xpath("//*[contains(normalize-space(),'" + firstName + "') or contains(normalize-space(),'" + lastName + "')]"))
                .waitUntilVisible()
                .isVisible();
    }

    private void waitForAddEmployeeForm() {
        for (int i = 0; i < 20; i++) {
            try {
                boolean firstNameVisible = findAll(firstNameInput).size() > 0;
                if (firstNameVisible) {
                    return;
                }
                waitABit(1000);
            } catch (Exception e) {
                waitABit(1000);
            }
        }
        throw new AssertionError("El formulario Add Employee no cargó correctamente");
    }

    private RemoteWebDriver getUnderlyingRemoteDriver() {
        WebDriver driver = getDriver();

        if (driver instanceof WebDriverFacade facade) {
            WebDriver proxied = facade.getProxiedDriver();
            if (proxied instanceof RemoteWebDriver remote) {
                return remote;
            }
        }

        if (driver instanceof RemoteWebDriver remote) {
            return remote;
        }

        throw new IllegalStateException(
                "El driver actual no es RemoteWebDriver. Tipo real: " + driver.getClass().getName()
        );
    }
}