Feature: User Authentication
  As a gaming cafe customer
  I want to log in to a station
  So that I can start a gaming session

  Scenario: Successful login with valid credentials
    Given the station agent is running
    When I log in with username "player1" and password "password"
    Then I should see the session screen
    And a session should be active

  Scenario: Failed login with empty username
    Given the station agent is running
    When I log in with username "" and password "password"
    Then I should see an error message "Please enter username and password."
    And no session should be active

  Scenario: Failed login with empty password
    Given the station agent is running
    When I log in with username "player1" and password ""
    Then I should see an error message "Please enter username and password."
    And no session should be active
