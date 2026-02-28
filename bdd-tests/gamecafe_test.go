package main

import (
	"testing"

	"github.com/amosdavis/gamecafe/bdd-tests/steps"
	"github.com/cucumber/godog"
)

func TestFeatures(t *testing.T) {
	suite := godog.TestSuite{
		Name: "gamecafe",
		ScenarioInitializer: func(ctx *godog.ScenarioContext) {
			steps.InitializeBillingScenario(ctx)
			steps.InitializeAuthScenario(ctx)
			steps.InitializeSessionScenario(ctx)
			steps.InitializeStationScenario(ctx)
		},
		Options: &godog.Options{
			Format:   "pretty",
			Paths:    []string{"features"},
			TestingT: t,
		},
	}

	if suite.Run() != 0 {
		t.Fatal("BDD feature tests failed")
	}
}
