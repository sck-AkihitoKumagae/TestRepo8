"""
Simple calculator module for basic arithmetic operations
"""

class Calculator:
    """A calculator class for performing basic arithmetic operations"""
    
    def __init__(self):
        self.history = []
    
    def add(self, a, b):
        """Add two numbers"""
        result = a + b
        self.history.append(f"{a} + {b} = {result}")
        return result
    
    def subtract(self, a, b):
        """Subtract b from a"""
        result = a - b
        self.history.append(f"{a} - {b} = {result}")
        return result
    
    def multiply(self, a, b):
        """Multiply two numbers"""
        result = a * b
        self.history.append(f"{a} * {b} = {result}")
        return result
    
    def divide(self, a, b):
        """Divide a by b"""
        if b == 0:
            raise ValueError("Cannot divide by zero")
        result = a / b
        self.history.append(f"{a} / {b} = {result}")
        return result
    
    def get_history(self):
        """Return calculation history"""
        return self.history

def main():
    """Main function to demonstrate calculator usage"""
    calc = Calculator()
    print("Calculator Example")
    print(calc.add(10, 5))
    print(calc.subtract(10, 5))
    print(calc.multiply(10, 5))
    print(calc.divide(10, 5))
    print("History:", calc.get_history())

if __name__ == "__main__":
    main()
