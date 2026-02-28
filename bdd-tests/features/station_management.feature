Feature: Station Management API
  As a gaming cafe operator
  I want to manage stations via the API
  So that I can monitor and control all gaming stations remotely

  Scenario: Registering a station via heartbeat
    Given the management server is running
    When station "station-1" sends a heartbeat with status "Available"
    Then the station should be registered in the system
    And the station status should be "Available"

  Scenario: Updating station status
    Given the management server is running
    And station "station-1" is registered
    When station "station-1" sends a heartbeat with status "InUse"
    Then the station status should be "InUse"

  Scenario: Getting all stations
    Given the management server is running
    And station "station-1" is registered
    And station "station-2" is registered
    When I request the list of all stations
    Then I should see "2" stations in the response

  Scenario: Getting revenue analytics
    Given the management server is running
    And a completed session exists with revenue "$10.00"
    When I request revenue analytics
    Then the total revenue should be at least "$10.00"
