package main

import (
	"flag"
	"fmt"
	"net/http"
	"strings"

	"github.com/google/uuid"
	"github.com/labstack/echo"
	"github.com/labstack/echo/middleware"

	"blockchain-go/blockchain"
)

func main() {
	portFlag := flag.Int("p", 5000, "port to listen on")
	flag.Parse()

	u, _ := uuid.NewRandom()
	nodeIdentifier := strings.ReplaceAll(u.String(), "-", "")

	b := blockchain.New()

	e := echo.New()
	e.Use(middleware.LoggerWithConfig(middleware.LoggerConfig{
		Format: "[${time_rfc3339}] ${remote_ip} - ${method} ${path} ${status}\n",
	}))
	e.Use(middleware.Recover())

	e.GET("/mine", func(c echo.Context) error {
		lastBlock := b.LastBlock()
		proof := blockchain.ProofOfWork(lastBlock)

		b.NewTransaction("0", nodeIdentifier, 1)

		previousHash := blockchain.Hash(lastBlock)
		block := b.NewBlock(proof, previousHash)

		response := map[string]interface{}{
			"message":       "New Block Forged",
			"index":         block.Index,
			"transactions":  block.Transactions,
			"proof":         block.Proof,
			"previous_hash": block.PreviousHash,
		}
		return c.JSON(http.StatusOK, response)
	})

	e.POST("/transactions/new", func(c echo.Context) (err error) {
		values := echo.Map{}
		if err = c.Bind(&values); err != nil {
			return
		}

		for _, key := range []string{"sender", "recipient", "amount"} {
			if _, ok := values[key]; !ok {
				return c.String(http.StatusBadRequest, "Missing values")
			}
		}

		index := b.NewTransaction(values["sender"].(string), values["recipient"].(string), int64(values["amount"].(float64)))

		response := map[string]interface{}{
			"message": fmt.Sprintf("Transaction will be added to Block %d", index),
		}
		return c.JSON(http.StatusCreated, response)
	})

	e.GET("/chain", func(c echo.Context) error {
		response := map[string]interface{}{
			"chain":  b.Chain,
			"length": len(b.Chain),
		}
		return c.JSON(http.StatusOK, response)
	})

	e.POST("/nodes/register", func(c echo.Context) (err error) {
		// values := echo.Map{}
		values := make(map[string][]string)
		if err = c.Bind(&values); err != nil {
			return
		}

		// nodes, ok := values["nodes"].([]string)
		nodes, ok := values["nodes"]
		if !ok {
			return c.String(http.StatusBadRequest, "Error: Please supply a valid list of nodes")
		}

		for _, node := range nodes {
			b.RegisterNode(node)
		}

		totalNodes := make([]string, 0)
		for node, _ := range b.Nodes {
			totalNodes = append(totalNodes, node)
		}
		response := map[string]interface{}{
			"message":     "New nodes have been added",
			"total_nodes": totalNodes,
		}
		return c.JSON(http.StatusCreated, response)
	})

	e.GET("/nodes/resolve", func(c echo.Context) error {
		replaced := b.ResolveConflicts()

		var response map[string]interface{}
		if replaced {
			response = map[string]interface{}{
				"message":   "Our chain was replaced",
				"new_chain": b.Chain,
			}
		} else {
			response = map[string]interface{}{
				"message": "Our chain is authoritative",
				"chain":   b.Chain,
			}
		}

		return c.JSON(http.StatusOK, response)
	})

	e.Logger.Fatal(e.Start(fmt.Sprintf(":%d", *portFlag)))
}
