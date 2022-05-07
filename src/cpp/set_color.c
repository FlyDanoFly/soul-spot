//
// Super basic C++ version to set the sun to a specific color
//
// 


#include <arpa/inet.h>
#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <sys/socket.h>
#include <unistd.h>


#define NUM_PIXELS 600

int main(int argc, char const* argv[])
{
    int sock = 0, valread;
    struct sockaddr_in serv_addr;
    char const* hello = "Hello from client";
    char buffer[1024] = { 0 };
    unsigned char mybuffer[3*600] = {0};
    if (argc != 5) {
        printf("usage: a.out port r g b\n");
	return -1;
    }

    int port = atoi(argv[1]);
    int r = atoi(argv[2]);
    int g = atoi(argv[3]);
    int b = atoi(argv[4]);
    if ((r < 0) || (r > 255)) {
	    printf("bad input for r, should be 0 <= r <= 255\n");
	    return -1;
    }
    if ((g < 0) || (g > 255)) {
	    printf("bad input for g, should be 0 <= r <= 255\n");
	    return -1;
    }
    if ((b < 0) || (b > 255)) {
	    printf("bad input for b, should be 0 <= r <= 255\n");
	    return -1;
    }
printf("%d %d %d %d\n", port, r, g, b);


    for (int i = 0; i < NUM_PIXELS; i++) {
        mybuffer[3*i+0] = r;
        mybuffer[3*i+1] = g;
        mybuffer[3*i+2] = b;
    }

    if ((sock = socket(AF_INET, SOCK_STREAM, 0)) < 0) {
        printf("\n Socket creation error \n");
        return -1;
    }
 
    serv_addr.sin_family = AF_INET;
    serv_addr.sin_port = htons(port);
 
    // Convert IPv4 and IPv6 addresses from text to binary
    // form
    if (inet_pton(AF_INET, "127.0.0.1", &serv_addr.sin_addr)
        <= 0) {
        printf(
            "\nInvalid address/ Address not supported \n");
        return -1;
    }
 
    if (connect(sock, (struct sockaddr*)&serv_addr,
                sizeof(serv_addr))
        < 0) {
        printf("\nConnection Failed \n");
        return -1;
    }
    printf("Sending hello message");
    send(sock, mybuffer, 3*NUM_PIXELS, 0);
    // send(sock, hello, strlen(hello), 0);
    printf("Hello message sent\n");
    //valread = read(sock, buffer, 1024);
    valread = read(sock, buffer, 4);
    int val = buffer[3] << 24 | buffer[2] << 16 | buffer[1] << 8 | buffer[0];
    printf("valread=%d, val=%d, %d %d %d %d\n", valread, val, buffer[0], buffer[1], buffer[2], buffer[3]);
    return 0;
}
