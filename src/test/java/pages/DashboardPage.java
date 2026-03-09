package pages;

import net.serenitybdd.core.pages.PageObject;
import net.serenitybdd.core.pages.WebElementFacade;
import org.openqa.selenium.By;

public class DashboardPage extends PageObject {

    private final By pimMenu = By.xpath("//span[text()='PIM']");
    private final By directoryMenu = By.xpath("//span[text()='Directory']");
    private final By dashboardHeader = By.xpath("//h6[contains(normalize-space(),'Dashboard')]");

    public void goToPim() {
        find(pimMenu).waitUntilClickable().click();
    }

    public void goToDirectory() {
        find(directoryMenu).waitUntilClickable().click();
    }

    public boolean isDashboardLoaded() {
        waitABit(5000);

        if (findAll(dashboardHeader).size() > 0) {
            return find(dashboardHeader).waitUntilVisible().isVisible();
        }

        return findAll(pimMenu).size() > 0 || findAll(directoryMenu).size() > 0;
    }
}