(define p0 (pascalesque '(lambda ((int i)) (* i i))))

(define p1 (pascalesque '(lambda ((int i)) (if (< i (int 20)) (* i i) (throw int (new-exception "Failure!"))))))

; test

(define p2 (pascalesque '(lambda ((int i))
      (let (
          ((func int int) proc (lambda ((int j))
              (if (<= j (int 20)) (* j j)
                (throw int (new-exception "Arg too big!"))))))
        (try-block
          (try (invoke proc i))
          (catch (exception e) (int -1)))))))

; test
