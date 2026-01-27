# Use an official Python base image
FROM python:3.11-slim

# Set the working directory inside the container
WORKDIR /code

# Copy the requirements file first to take advantage of Docker caching
COPY ./requirements.txt /code/requirements.txt

# Install the dependencies
RUN pip install --no-cache-dir --upgrade -r /code/requirements.txt

# Copy the rest of your application code
COPY . /code

# Command to run the Uvicorn server
# Note: We use 0.0.0.0 so it can be accessed from outside the container
CMD ["uvicorn", "main:app", "--host", "0.0.0.0", "--port", "6789"]