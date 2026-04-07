Feature: Create booking
  As a booking client
  I want booking creation to respect fully occupied ranges and date validation
  So that only valid and available periods are accepted

  Background:
    Given a hotel with 2 rooms
    And a fully occupied range from 10 to 12 days from today

  Scenario Outline: Decision table booking outcomes
    Given a booking request from <StartOffset> to <EndOffset> days from today
    When I create the booking
    Then booking should <Outcome>

    Examples:
      | StartOffset | EndOffset | Outcome |
      | 8           | 9         | succeed |
      | 13          | 14        | succeed |
      | 9           | 13        | fail    |
      | 9           | 10        | fail    |
      | 9           | 12        | fail    |
      | 10          | 13        | fail    |
      | 12          | 13        | fail    |
      | 10          | 10        | fail    |
      | 10          | 12        | fail    |
      | 12          | 12        | fail    |

  Scenario Outline: Invalid equivalence classes are rejected
    Given a booking request from <StartOffset> to <EndOffset> days from today
    When I create the booking
    Then an ArgumentException should be thrown

    Examples:
      | StartOffset | EndOffset |
      | 0           | 1         |
      | -1          | 1         |
      | 5           | 4         |

  Scenario: Optional - fully occupied dates are calculated correctly
    When I request fully occupied dates from 9 to 13 days from today
    Then fully occupied dates should be 10, 11, 12 days from today