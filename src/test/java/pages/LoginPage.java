package pages;

import net.serenitybdd.core.pages.PageObject;
import net.serenitybdd.core.pages.WebElementFacade;
import org.openqa.selenium.By;
import org.openqa.selenium.support.FindBy;

public class LoginPage extends PageObject {

    @FindBy(name = "username")
    private WebElementFacade txtUsername;

    @FindBy(name = "password")
    private WebElementFacade txtPassword;

    @FindBy(css = "button[type='submit']")
    private WebElementFacade btnLogin;

    private final By usernameLocator = By.name("username");
    private final By passwordLocator = By.name("password");

    public void openLoginPage() {
        openUrl("https://opensource-demo.orangehrmlive.com/");

        waitForLoginPageToBeReady();
    }

    public void login(String username, String password) {
        find(usernameLocator).waitUntilVisible().type(username);
        find(passwordLocator).waitUntilVisible().type(password);
        btnLogin.waitUntilClickable().click();
    }

    public boolean isLoginPageLoaded() {
        return findAll(usernameLocator).size() > 0;
    }

    private void waitForLoginPageToBeReady() {
        for (int i = 0; i < 20; i++) {
            try {
                String currentUrl = getDriver().getCurrentUrl();

                boolean validUrl =
                        currentUrl != null &&
                                !currentUrl.isBlank() &&
                                !currentUrl.startsWith("data:");

                boolean usernameVisible = findAll(usernameLocator).size() > 0;

                if (validUrl && usernameVisible) {
                    return;
                }

                waitABit(1000);
            } catch (Exception e) {
                waitABit(1000);
            }
        }

        throw new AssertionError("La página de login no mostró el campo username a tiempo");
    }
}