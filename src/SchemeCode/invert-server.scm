; This file is part of Sunlit World Scheme
; Copyright (c) 2010 by Edward Kiser (edkiser@gmail.com)
;
; This program is free software; you can redistribute it and/or modify
; it under the terms of the GNU General Public License as published by
; the Free Software Foundation; either version 2 of the License, or
; (at your option) any later version.
;
; This program is distributed in the hope that it will be useful,
; but WITHOUT ANY WARRANTY; without even the implied warranty of
; MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
; GNU General Public License for more details.
;
; You should have received a copy of the GNU General Public License along
; with this program; if not, write to the Free Software Foundation, Inc.,
; 51 Franklin Street, Fifth Floor, Boston, MA 02110-1301 USA.

(define async-result (lambda (ar)
    (let* (
        (result (hashmap-ref ar 'result)))
      (if (hashmap-ref ar 'exception?) (throw result) result))))

(define tcp-send! (lambda (socket mybyterange)
    (async-result (wait-any (begin-tcp-send! socket mybyterange)))))

(define tcp-receive! (lambda (socket mybyterange)
    (async-result (wait-any (begin-tcp-receive! socket mybyterange)))))

(define send-byte-packet! (lambda (socket mybyterange)
    (let* (
        (length-bytes (make-bytes 2)))
      (byte-set-hbla! length-bytes #t)
      (byte-set-uint-s! (byterange length-bytes 0 2) (byterange-length mybyterange))
      (tcp-send! socket (byterange length-bytes))
      (if (> (byterange-length mybyterange) 0) (tcp-send! socket mybyterange)))))

(define receive-byte-packet! (lambda (socket)
    (let* (
        (length-bytes (make-bytes 2)))
      (byte-set-hbla! length-bytes #t)
      (tcp-receive! socket (byterange length-bytes))
      (let* (
          (len (byte-ref-uint (byterange length-bytes 0 2)))
          (data-bytes (make-bytes len)))
        (if (> len 0) (tcp-receive! socket (byterange data-bytes)))
        data-bytes))))

(define invert-bytes (lambda (ba)
    (let* (
        (len (byte-length ba))
        (bb (make-bytes len)))
      (let loop ((i 0))
        (if (>= i len) bb
          (begin
            (byte-set! bb i (logxor 255 (byte-ref ba i)))
            (loop (+ i 1))))))))
    
(define begin-server-client-thread (lambda (socket)
    (begin-thread (lambda ()
        (let loop ()
          (let* (
              (rd (receive-byte-packet! socket)))
            (if (= (byte-length rd) 0)
              (begin
                (send-byte-packet! socket (byterange rd 0 0))
                (dispose! socket)
                #t)
              (begin
                (send-byte-packet! socket (byterange (invert-bytes rd)))
                (loop)))))))))

(define list-remove (lambda (item lst)
    (let loop (
        (lst lst)
        (result '()))
      (if (null? lst) (reverse result)
        (let* (
            (item1 (car lst))
            (lst (cdr lst)))
          (if (eq? item item1) (loop lst result)
            (loop lst (cons item1 result))))))))

(define begin-server (lambda (ipaddr port)
    (let* (
        (serversocket (make-tcp-server ipaddr port 2))
        (command-queue (make-async-queue)))
      (letrec (
          (run (lambda (await-accept await-command await-thread-deaths)
              (let* (
                  (waitresult (apply wait-any (cons await-accept (cons await-command await-thread-deaths))))
                  (id (hashmap-ref waitresult 'id)))
                (cond
                  ((eq? id await-accept)
                    (run
                      (begin-tcp-accept! serversocket)
                      await-command
                      (cons (begin-server-client-thread (async-result waitresult)) await-thread-deaths)))
                  ((eq? id await-command)
                    (let* (
                        (command (async-result waitresult)))
                      (if (and (hashmap? command) (hashmap-contains-key? command 'command))
                        (let* (
                            (cmdid (hashmap-ref command 'command)))
                          (cond
                            ((eq? cmdid 'stop)
                              (dispose! serversocket)
                              (dispose! command-queue)
                              (async-queue-put! (hashmap-ref command 'k) #t)
                              (shutdown-1 await-accept await-thread-deaths))
                            ((eq? cmdid 'status)
                              (async-queue-put! (hashmap-ref command 'k) (list-length await-thread-deaths))
                              (run await-accept (begin-async-queue-get! command-queue) await-thread-deaths))
                            (else
                              (run await-accept (begin-async-queue-get! command-queue) await-thread-deaths))))
                        (run await-accept (begin-async-queue-get! command-queue) await-thread-deaths))))
                  (else
                    (run await-accept await-command (list-remove id await-thread-deaths)))))))
           (shutdown-1 (lambda (await-accept await-thread-deaths)
               (let* (
                   (waitresult (apply wait-any (cons await-accept await-thread-deaths)))
                   (id (hashmap-ref waitresult 'id)))
                 (cond
                   ((eq? id await-accept)
                     (shutdown-2 await-thread-deaths))
                   (else
                     (shutdown-1 await-accept (list-remove id await-thread-deaths)))))))
           (shutdown-2 (lambda (await-thread-deaths)
               (if (null? await-thread-deaths) #t
                 (let* (
                     (waitresult (apply wait-any await-thread-deaths))
                     (id (hashmap-ref waitresult 'id)))
                   (shutdown-2 (list-remove id await-thread-deaths)))))))
         (let* (
             (thread (begin-thread (lambda ()
                   (run
                     (begin-tcp-accept! serversocket)
                     (begin-async-queue-get! command-queue)
                     '()))))
             (h (make-hashmap)))
           (hashmap-set! h 'thread thread)
           (hashmap-set! h 'command-queue command-queue)
           h)))))

(define result-queue (make-async-queue))

(define get-status (lambda (server)
    (let* (
        (message (make-hashmap)))
      (hashmap-set! message 'command 'status)
      (hashmap-set! message 'k result-queue)
      (async-queue-put! (hashmap-ref server 'command-queue) message)
      (async-result (wait-any (begin-async-queue-get! result-queue))))))

(define stop (lambda (server)
    (let* (
        (message (make-hashmap)))
      (hashmap-set! message 'command 'stop)
      (hashmap-set! message 'k result-queue)
      (async-queue-put! (hashmap-ref server 'command-queue) message)
      (async-result (wait-any (begin-async-queue-get! result-queue))))))

(define connect (lambda (ipaddr port)
    (async-result (wait-any (begin-tcp-connect! ipaddr port)))))

(define invert-via (lambda (socket br)
    (send-byte-packet! socket br)
    (receive-byte-packet! socket)))
