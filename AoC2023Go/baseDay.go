package main

import (
	"bufio"
	"log"
	"os"
)

type BaseDay struct {
	filepath string
}

func (bd *BaseDay) SetFilePath(filepath string) {
	bd.filepath = filepath
}

func (bd *BaseDay) ReadFileAndParse(filepath string, parseLine func(string)) {
	f, err := os.Open(filepath)
	if err != nil {
		log.Fatal(err)
	}
	// remember to close the file at the end of the program
	defer f.Close()

	// read the file line by line using scanner
	scanner := bufio.NewScanner(f)

	for scanner.Scan() {
		line := scanner.Text()
		parseLine(line)
		//		d.lines = append(d.lines, scanner.Text())
	}
	parseLine("")

	if err := scanner.Err(); err != nil {
		log.Fatal(err)
	}
}

func Reverse(toReverse string) string {
	n := 0
	rune := make([]rune, len(toReverse))
	for _, r := range toReverse {
		rune[n] = r
		n++
	}
	rune = rune[0:n]
	// Reverse
	for i := 0; i < n/2; i++ {
		rune[i], rune[n-1-i] = rune[n-1-i], rune[i]
	}
	return string(rune)
}
