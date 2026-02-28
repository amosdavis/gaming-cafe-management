Feature: Session Management
  As a gaming cafe operator
  I want sessions to be tracked accurately
  So that billing is applied correctly

  Scenario: Starting a session records start time
    Given the session service is initialized
    When I start a session for user "42" on station "1"
    Then the session status should be "Active"
    And the session start time should be recorded

  Scenario: Ending a session records end time
    Given the session service is initialized
    And an active session exists for user "42" on station "1"
    When I end the session
    Then the session status should be "Completed"
    And the session end time should be recorded

  Scenario: Only one active session per station
    Given the session service is initialized
    And an active session exists for user "42" on station "1"
    When I start a second session for user "99" on station "1"
    Then only one active session should exist for station "1"

  Scenario: Session duration is calculated correctly
    Given the session service is initialized
    And a session started "30" minutes ago
    When I calculate the session duration
    Then the duration should be approximately "30" minutes
