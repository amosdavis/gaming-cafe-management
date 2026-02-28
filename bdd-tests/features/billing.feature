Feature: Billing Calculation
  As a gaming cafe operator
  I want the billing engine to calculate charges accurately
  So that customers are charged correctly

  Scenario: Hourly billing rounds up partial hours
    Given a billing rate of "$5.00" per hour
    When a session lasts "30" minutes
    Then the charge should be "$5.00"

  Scenario: Hourly billing charges exact hours
    Given a billing rate of "$5.00" per hour
    When a session lasts "60" minutes
    Then the charge should be "$5.00"

  Scenario: Hourly billing rounds up 61 minutes to 2 hours
    Given a billing rate of "$5.00" per hour
    When a session lasts "61" minutes
    Then the charge should be "$10.00"

  Scenario: Per-minute billing is exact
    Given a billing rate of "$0.10" per minute
    When a session lasts "45" minutes
    Then the charge should be "$4.50"

  Scenario: Zero-duration session has no charge
    Given a billing rate of "$5.00" per hour
    When a session lasts "0" minutes
    Then the charge should be "$0.00"

  Scenario: Flat-rate billing ignores duration
    Given a flat rate of "$10.00"
    When a session lasts "120" minutes
    Then the charge should be "$10.00"
