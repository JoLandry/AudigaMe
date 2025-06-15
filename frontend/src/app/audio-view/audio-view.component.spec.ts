import { ComponentFixture, TestBed } from '@angular/core/testing';

import { AudioViewComponent } from './audio-view.component';

describe('AudioViewComponent', () => {
  let component: AudioViewComponent;
  let fixture: ComponentFixture<AudioViewComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AudioViewComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(AudioViewComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
