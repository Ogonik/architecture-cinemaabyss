FROM golang@sha256:9b07790bb99f0cba351ed7199a421b6281b0d84325647c83d009f6540f6ceb90 AS builder

WORKDIR /app

# Copy go mod and sum files
COPY go.mod ./

# Download all dependencies
RUN go mod download

# Copy the source code
COPY . .

# Build the application
RUN CGO_ENABLED=0 GOOS=linux go build -a -installsuffix cgo -o movies-service .

# Use a smaller image for the final application
FROM alpine@sha256:4bcff63911fcb4448bd4fdacec207030997caf25e9bea4045fa6c8c44de311d1   

RUN apk --no-cache add ca-certificates

WORKDIR /root/

# Copy the binary from builder
COPY --from=builder /app/movies-service .

# Expose the application port
EXPOSE 8081

# Command to run the executable
CMD ["./movies-service"]