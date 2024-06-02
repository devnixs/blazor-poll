import { Min, MinLength } from 'class-validator';

export class BodyDto {
  @MinLength(1)
  name: string;

  @Min(1)
  age: number;
}
