package pages;

import net.serenitybdd.core.pages.PageObject;
import net.serenitybdd.core.pages.WebElementFacade;
import org.openqa.selenium.By;

public class DirectoryPage extends PageObject {

    private final By employeeNameInput = By.xpath("(//input[@placeholder='Type for hints...'])[1]");
    private final By firstSuggestion = By.xpath("//div[@role='listbox']//span");
    private final By searchButton = By.cssSelector("button[type='submit']");

    public void searchEmployee(String firstName) {
        WebElementFacade input = find(employeeNameInput);
        input.waitUntilVisible().clear();
        input.type(firstName);

        find(firstSuggestion).waitUntilVisible();
        find(firstSuggestion).waitUntilClickable().click();

        waitABit(1000);

        find(searchButton).waitUntilClickable().click();
        waitABit(3000);
    }

    public boolean employeeAppearsInResults(String firstName, String lastName) {
        return findAll(By.xpath("//*[contains(normalize-space(),'" + firstName + "') or contains(normalize-space(),'" + lastName + "')]")).size() > 0;
    }
}