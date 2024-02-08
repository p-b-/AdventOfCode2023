package main

func main() {
	d := NewDayWithTestParameters(23, false, 0)
	partNumber := 1
	d.readFile(partNumber)
	d.execute(partNumber)
}
